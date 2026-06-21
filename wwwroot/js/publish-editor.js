(() => {
  const sanitizePaste = event => {
    event.preventDefault();
    const text = event.clipboardData?.getData('text/plain') ?? '';
    document.execCommand('insertText', false, text);
  };

  const countWords = html => {
    const temp = document.createElement('div');
    temp.innerHTML = html;
    const text = temp.textContent?.trim() ?? '';
    return text ? text.split(/\s+/).length : 0;
  };

  document.querySelectorAll('[data-rich-editor]').forEach(editor => {
    const form = editor.closest('form');
    const surface = editor.querySelector('[data-rich-editor-surface]');
    const output = form?.querySelector('[data-rich-editor-output]');
    const counter = form?.querySelector('[data-rich-word-count]');
    const format = editor.querySelector('[data-rich-format]');

    if (!form || !surface || !output) return;

    const commandButtons = [...editor.querySelectorAll('[data-rich-command]')];
    const inlineCommands = ['bold', 'italic', 'underline'];
    const stickyInlineCommands = new Set();

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
      output.value = surface.innerHTML.trim();
      if (counter) counter.textContent = countWords(output.value).toString();
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
        updateToolbarState();
      });
    });

    format?.addEventListener('change', () => {
      surface.focus();
      document.execCommand('formatBlock', false, format.value);
      sync();
      updateToolbarState();
    });

    surface.addEventListener('keydown', event => {
      if (event.key !== 'Enter' || event.shiftKey) return;
      window.setTimeout(() => {
        resetInlineFormatting();
        document.execCommand('formatBlock', false, 'p');
        ensureStickyFormatting();
        sync();
      }, 0);
    });

    surface.addEventListener('input', () => {
      ensureStickyFormatting();
      sync();
      updateToolbarState();
    });
    surface.addEventListener('keyup', updateToolbarState);
    surface.addEventListener('mouseup', updateToolbarState);
    surface.addEventListener('paste', sanitizePaste);
    form.addEventListener('submit', sync);
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
    updateToolbarState();
  });
})();
