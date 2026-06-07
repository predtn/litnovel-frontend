Actor	Code	Use Case Name	Description	Group
Guest	UC-U01	Register Account	Create a new user account.	Auth & Profile
Guest	UC-U02	Login	Authenticate and access the system.	Auth & Profile
Guest	UC-U03	Logout	Invalidate session/tokens and sign out.	Auth & Profile
User	UC-U04	Update Profile	Edit avatar, display name, and bio.	Auth & Profile
User	UC-U05	View Novel List	Browse novels with filters (category, tag, status, etc.).	Reading
User	UC-U06	Search Novels	Search for novels by keyword (title, author, description).	Reading
User	UC-U07	View Novel Details	View novel metadata, volume list, chapter list, and comments.	Reading
User	UC-U08	Read Chapter	View chapter content and increase view counts.	Reading
User	UC-U09	Continue Reading	Jump to the last read chapter using ReadingProgress.	Reading
User	UC-U10	Favorite Novel	Add a novel to the favorites/following list.	Interaction
User	UC-U11	Unfavorite Novel	Remove a novel from the favorites list.	Interaction
User	UC-U12	Rate & Review Novel	Submit a star rating and text review for a novel.	Interaction
User	UC-U13	Comment on Novel	Post a comment on the novel's main page.	Interaction
User	UC-U14	Comment on Chapter	Post a comment on a specific chapter.	Interaction
User	UC-U15	Like Novels	Like a specific novel	Interaction
User	UC-U16	View like novels history	View liked novel history 	Interaction
User	UC-U17	Reply to Comment	Reply to an existing comment (nested thread).	Interaction
User	UC-U18	Delete Own Comment	Soft-delete a user's own comment.	Interaction
User	UC-U19	Report Violation	Report a novel, chapter, comment, or forum post/thread.	Interaction
User	UC-U20	Create Novel	Upload a new novel (sets status to PENDING).	Publishing
User	UC-U21	Update Own Novel	Edit details of a published or pending novel.	Publishing
User	UC-U22	Delete Own Novel	Delete a published or pending novel.	Publishing
User	UC-U23	Create Volume	Add a new volume to an owned novel.	Publishing
User	UC-U24	Update Own Volume	Edit details or order of an owned volume.	Publishing
User	UC-U25	Delete Own Volume	Delete a volume (only if no chapters exist).	Publishing
User	UC-U26	Add Chapter	Add a new chapter to a volume (sets status to PENDING).	Publishing
User	UC-U27	Update Own Chapter	Edit chapter title or content.	Publishing
User	UC-U28	Delete Own Chapter	Soft-delete or hide an owned chapter.	Publishing
User	UC-U29	View Own Novels	Manage uploaded novels (My Novels dashboard).	Publishing
User	UC-U30	View Moderation Status	Track pending/approved/rejected status of own content.	Publishing
User	UC-U31	View Forum Categories	Browse forum boards and categories.	Forum
User	UC-U32	View Forum Threads	Browse threads within a category, sorted by pins/votes/date.	Forum
User	UC-U33	Create Forum Thread	Post a new thread (supports flairs and linking to a novel).	Forum
User	UC-U34	Update Own Thread	Edit title, content, or flair of an owned thread.	Forum
User	UC-U35	Create Forum Post	Reply to a thread or another post (nested replies).	Forum
User	UC-U36	Update Own Post	Edit an owned forum post.	Forum
User	UC-U37	Delete Own Thread/Post	Soft-delete an owned thread or post.	Forum
User	UC-U38	Vote on Thread/Post	Upvote or downvote a forum thread or post.	Forum
Staff	UC-S01	View Moderation Dashboard	View queue of pending novels, chapters, and open reports.	Moderation
Staff	UC-S02	Approve Novel	Review and publish a pending novel.	Moderation
Staff	UC-S03	Reject Novel	Reject a pending novel with a reason.	Moderation
Staff	UC-S04	Lock Novel	Hide or lock a published novel due to severe violations.	Moderation
Staff	UC-S05	Approve Chapter	Review and publish a pending chapter.	Moderation
Staff	UC-S06	Reject Chapter	Reject a pending chapter with a reason.	Moderation
Staff	UC-S07	Lock Chapter	Block public access to a specific violating chapter.	Moderation
Staff	UC-S08	Handle Novel Report	Resolve reports targeting a novel.	Moderation
Staff	UC-S09	Handle Chapter Report	Resolve reports targeting a chapter.	Moderation
Staff	UC-S10	Handle Comment Report	Resolve reports targeting a comment.	Moderation
Staff	UC-S11	Delete/Hide Comment	Soft-delete a violating comment.	Moderation
Staff	UC-S12	Handle Forum Report	Resolve reports targeting forum threads or posts.	Forum Mod
Staff	UC-S13	Pin/Unpin Thread	Sticky or unsticky a forum thread.	Forum Mod
Staff	UC-S14	Lock/Unlock Thread	Prevent new posts on a forum thread.	Forum Mod
Staff	UC-S15	Delete/Hide Thread/Post	Moderation deletion of violating forum threads or posts.	Forum Mod
Staff	UC-S16	Warn User	Send an official warning notification to a user.	User Mod
Staff	UC-S17	View Moderation History	View personal audit logs of processed actions.	Audit
Admin	UC-A01	Manage Users	View and search the entire user base.	User Mgmt
Admin	UC-A02	Ban/Unban User	Lock or unlock user accounts (revokes tokens).	User Mgmt
Admin	UC-A03	Assign Staff Role	Promote a USER to STAFF.	User Mgmt
Admin	UC-A04	Revoke Staff Role	Demote a STAFF member back to USER.	User Mgmt
Admin	UC-A05	Manage User Badges	Award or remove reputation/achievement badges.	User Mgmt
Admin	UC-A06	Manage Novel Categories	Create, update, or delete novel categories (parent/child).	System Data
Admin	UC-A07	Manage Tags	Create, update, merge, or delete tags.	System Data
Admin	UC-A08	Manage Forum Categories	Create, order, or delete forum boards.	System Data
Admin	UC-A09	Manage Forum Flairs	Create or manage flairs specific to forum categories.	System Data
Admin	UC-A10	Manage System Notifications	Broadcast notifications to all users, staff, or individuals.	System Data
Admin	UC-A11	View All Reports	Oversee all reports, including those processed by Staff.	Audit
Admin	UC-A12	View System Logs	View full audit logs (Actor, Action, Entity, IP, Metadata).	Audit
Admin	UC-A13	View System Statistics	Dashboard for total users, chapters, views, and pending queues.	Audit
Admin	UC-A14	High-level Novel Override	Force edit, transfer ownership, or delete any novel.	Override
Admin	UC-A15	High-level Chapter Override	Force edit, move, or restore any chapter.	Override