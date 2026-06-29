(() => {
  const sanitizePaste = event => {
    event.preventDefault();
    const text = event.clipboardData?.getData('text/plain') ?? '';
    document.execCommand('insertText', false, text);
  };

  const getPlainText = html => {
    const temp = document.createElement('div');
    temp.innerHTML = html;
    temp.querySelectorAll('br').forEach(node => node.replaceWith(document.createTextNode(' ')));
    temp
      .querySelectorAll('p,h1,h2,h3,h4,h5,h6,li,blockquote,div,section,article')
      .forEach(node => node.appendChild(document.createTextNode(' ')));
    return (temp.textContent ?? '')
      .replace(/\u00a0/g, ' ')
      .replace(/[\u200B-\u200D\uFEFF]/g, '')
      .replace(/\s+/g, ' ')
      .trim();
  };

  const countWords = html => {
    const text = getPlainText(html);
    const words = text.match(/[\p{L}\p{N}]+(?:['’.-][\p{L}\p{N}]+)*/gu);
    return words?.length ?? 0;
  };

  document.querySelectorAll('[data-rich-editor]').forEach(editor => {
    const form = editor.closest('form');
    const surface = editor.querySelector('[data-rich-editor-surface]');
    const output = form?.querySelector('[data-rich-editor-output]');
    const counter = form?.querySelector('[data-rich-word-count]');
    const format = editor.querySelector('[data-rich-format]');
    const titleInput = form?.querySelector('[name="Input.Title"]');
    const chapterNumberInput = form?.querySelector('[name="Input.ChapterNumber"]');
    const editorMeta = form?.querySelector('.publish-editor-meta');

    if (!form || !surface || !output) return;

    const commandButtons = [...editor.querySelectorAll('[data-rich-command]')];
    const inlineCommands = ['bold', 'italic', 'underline'];
    const stickyInlineCommands = new Set();
    const autosaveKey = `litnovel:publish-editor:${window.location.pathname}`;
    const autosaveDelay = 800;
    let autosaveTimer = null;
    let autosaveStatus = null;

    if (editorMeta) {
      autosaveStatus = document.createElement('span');
      autosaveStatus.className = 'text-caption text-mute';
      autosaveStatus.setAttribute('data-autosave-status', '');
      editorMeta.appendChild(autosaveStatus);
    }

    const getSelection = () => {
      const selection = document.getSelection();
      if (!selection || selection.rangeCount === 0) return null;
      const node = selection.anchorNode;
      const container = node?.nodeType === Node.ELEMENT_NODE ? node : node?.parentNode;
      return container && surface.contains(container)
        ? selection
        : null;
    };

    const hasSelectedText = () => {
      const selection = getSelection();
      return !!selection && !selection.isCollapsed;
    };

    const sync = () => {
      const html = surface.innerHTML.trim();
      output.value = getPlainText(html) ? html : '';
      if (counter) counter.textContent = countWords(output.value).toString();
    };

    const setAutosaveStatus = text => {
      if (autosaveStatus) autosaveStatus.textContent = text;
    };

    const getAutosaveDraft = () => {
      try {
        return JSON.parse(window.localStorage.getItem(autosaveKey) || 'null');
      } catch {
        return null;
      }
    };

    const clearAutosaveDraft = () => {
      window.localStorage.removeItem(autosaveKey);
      setAutosaveStatus('');
    };

    const saveAutosaveDraft = () => {
      sync();
      const draft = {
        content: output.value,
        title: titleInput?.value ?? '',
        chapterNumber: chapterNumberInput?.value ?? '',
        savedAt: new Date().toISOString()
      };

      if (!draft.content && !draft.title && !draft.chapterNumber) {
        clearAutosaveDraft();
        return;
      }

      window.localStorage.setItem(autosaveKey, JSON.stringify(draft));
      setAutosaveStatus('Đã tự lưu');
    };

    const scheduleAutosave = () => {
      window.clearTimeout(autosaveTimer);
      setAutosaveStatus('Đang tự lưu...');
      autosaveTimer = window.setTimeout(saveAutosaveDraft, autosaveDelay);
    };

    const restoreAutosaveDraft = () => {
      const draft = getAutosaveDraft();
      if (!draft || (!draft.content && !draft.title && !draft.chapterNumber)) return;

      sync();
      const hasDifferentContent = draft.content && draft.content !== output.value;
      const hasDifferentTitle = titleInput && draft.title && draft.title !== titleInput.value;
      const hasDifferentChapterNumber = chapterNumberInput && draft.chapterNumber && draft.chapterNumber !== chapterNumberInput.value;
      if (!hasDifferentContent && !hasDifferentTitle && !hasDifferentChapterNumber) return;

      const savedAt = draft.savedAt ? new Date(draft.savedAt).toLocaleString() : '';
      const message = savedAt
        ? `Có bản nháp tự lưu lúc ${savedAt}. Khôi phục bản nháp này?`
        : 'Có bản nháp tự lưu. Khôi phục bản nháp này?';

      if (!window.confirm(message)) return;

      if (draft.content) surface.innerHTML = draft.content;
      if (titleInput && draft.title) titleInput.value = draft.title;
      if (chapterNumberInput && draft.chapterNumber) chapterNumberInput.value = draft.chapterNumber;
      sync();
      setAutosaveStatus('Đã khôi phục bản nháp');
    };

    const updateToolbarState = () => {
      commandButtons.forEach(button => {
        const command = button.dataset.richCommand;
        const active = command ? document.queryCommandState(command) : false;
        button.classList.toggle('active', active);
        button.setAttribute('aria-pressed', active ? 'true' : 'false');
      });

      if (format) {
        const block = (document.queryCommandValue('formatBlock') || 'p').toLowerCase().replace(/[<>]/g, '');
        format.value = ['h2', 'h3', 'blockquote'].includes(block) ? block : 'p';
      }
    };

    const resetInlineFormatting = () => {
      inlineCommands.forEach(command => {
        if (!stickyInlineCommands.has(command) && document.queryCommandState(command)) {
          document.execCommand(command, false);
        }
      });
      updateToolbarState();
    };

    const ensureStickyFormatting = () => {
      stickyInlineCommands.forEach(command => {
        if (!document.queryCommandState(command)) {
          document.execCommand(command, false);
        }
      });
      updateToolbarState();
    };

    const collapseSelectionToEnd = () => {
      const selection = getSelection();
      if (!selection || selection.rangeCount === 0) return;
      const range = selection.getRangeAt(0);
      range.collapse(false);
      selection.removeAllRanges();
      selection.addRange(range);
    };

    commandButtons.forEach(button => {
      button.addEventListener('click', () => {
        surface.focus();
        const command = button.dataset.richCommand;
        if (!command) return;

        if (inlineCommands.includes(command) && !hasSelectedText()) {
          if (stickyInlineCommands.has(command)) {
            stickyInlineCommands.delete(command);
          } else {
            stickyInlineCommands.add(command);
          }
          document.execCommand(command, false);
        } else {
          document.execCommand(command, false);
          if (inlineCommands.includes(command)) {
            collapseSelectionToEnd();
            inlineCommands.forEach(inlineCommand => {
              if (document.queryCommandState(inlineCommand)) {
                document.execCommand(inlineCommand, false);
              }
            });
          }
        }

        sync();
        scheduleAutosave();
        updateToolbarState();
      });
    });

    format?.addEventListener('change', () => {
      surface.focus();
      document.execCommand('formatBlock', false, format.value);
      sync();
      scheduleAutosave();
      updateToolbarState();
    });

    surface.addEventListener('keydown', event => {
      if (event.key !== 'Enter' || event.shiftKey) return;
      window.setTimeout(() => {
        resetInlineFormatting();
        document.execCommand('formatBlock', false, 'p');
        ensureStickyFormatting();
        sync();
        scheduleAutosave();
      }, 0);
    });

    surface.addEventListener('input', () => {
      ensureStickyFormatting();
      sync();
      scheduleAutosave();
      updateToolbarState();
    });
    titleInput?.addEventListener('input', scheduleAutosave);
    chapterNumberInput?.addEventListener('input', scheduleAutosave);
    surface.addEventListener('keyup', updateToolbarState);
    surface.addEventListener('mouseup', updateToolbarState);
    surface.addEventListener('paste', sanitizePaste);
    form.addEventListener('submit', () => {
      sync();
      clearAutosaveDraft();
    });
    document.addEventListener('selectionchange', () => {
      if (document.activeElement === surface || surface.contains(document.getSelection()?.anchorNode)) {
        updateToolbarState();
      }
    });

    if (!surface.innerHTML.trim()) {
      surface.innerHTML = '<h2><br></h2>';
      if (format) format.value = 'h2';
    }

    sync();
    restoreAutosaveDraft();
    sync();
    updateToolbarState();
  });
})();
