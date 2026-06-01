import { useState, useEffect, useCallback } from 'react';
import { discussionsApi } from '../api/discussions';
import { useAuth } from '../auth/useAuth';
import type { DiscussionThread, Comment, UserInfo } from '../types/discussion';

interface Props {
  courseId: number;
}

export function CourseDiscussions({ courseId }: Props) {
  const { email, name, roles } = useAuth();
  const [threads, setThreads] = useState<DiscussionThread[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [expandedThread, setExpandedThread] = useState<string | null>(null);
  const [showNewThread, setShowNewThread] = useState(false);
  const [newThreadTitle, setNewThreadTitle] = useState('');
  const [newThreadSubmitting, setNewThreadSubmitting] = useState(false);

  const [commentText, setCommentText] = useState<Record<string, string>>({});
  const [commentSubmitting, setCommentSubmitting] = useState<string | null>(null);
  const [commentError, setCommentError] = useState<string | null>(null);
  const [editingComment, setEditingComment] = useState<{ threadId: string; commentId: string; text: string } | null>(null);
  const [editSubmitting, setEditSubmitting] = useState(false);
  const [deletingComment, setDeletingComment] = useState<string | null>(null);
  const [togglingThread, setTogglingThread] = useState<string | null>(null);

  const canWrite = !!email;
  const isTeacher = roles.some(r => r.toLowerCase() === 'teacher' || r.toLowerCase() === 'admin');

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await discussionsApi.getByTarget(String(courseId), 0);
      setThreads(data);
    } catch {
      setError('Failed to load discussions.');
    } finally {
      setLoading(false);
    }
  }, [courseId]);

  useEffect(() => { load(); }, [load]);

  function buildUserInfo(): UserInfo | null {
    if (!email) return null;
    return {
      userId: email,
      userName: name ?? email.split('@')[0],
      role: { name: roles[0] ?? 'Student' },
    };
  }

  async function handleCreateThread() {
    if (!newThreadTitle.trim()) return;
    const userInfo = buildUserInfo();
    if (!userInfo) return;
    setNewThreadSubmitting(true);
    try {
      await discussionsApi.createThread(String(courseId), 0, newThreadTitle.trim(), userInfo);
      setNewThreadTitle('');
      setShowNewThread(false);
      await load();
    } catch {
      // thread creation errors are rare; the form stays open so user can retry
    } finally {
      setNewThreadSubmitting(false);
    }
  }

  async function handleAddComment(threadId: string) {
    const text = commentText[threadId]?.trim();
    if (!text) return;
    const userInfo = buildUserInfo();
    if (!userInfo) return;
    setCommentSubmitting(threadId);
    setCommentError(null);
    try {
      const comment: Omit<Comment, 'id' | 'createdAt' | 'updatedAt'> = {
        author: userInfo,
        content: text,
        isResolved: false,
      };
      await discussionsApi.addComment(threadId, comment);
      setCommentText(prev => ({ ...prev, [threadId]: '' }));
      await load();
    } catch {
      setCommentError('Failed to post comment. Please try again.');
    } finally {
      setCommentSubmitting(null);
    }
  }

  async function handleEditComment() {
    if (!editingComment || !editingComment.text.trim()) return;
    const userInfo = buildUserInfo();
    if (!userInfo) return;
    setEditSubmitting(true);
    try {
      await discussionsApi.editComment(editingComment.threadId, editingComment.commentId, editingComment.text.trim(), userInfo);
      setEditingComment(null);
      await load();
    } catch {
      // stays open so user can retry
    } finally {
      setEditSubmitting(false);
    }
  }

  async function handleDeleteComment(threadId: string, commentId: string) {
    const userInfo = buildUserInfo();
    if (!userInfo) return;
    setDeletingComment(commentId);
    try {
      await discussionsApi.deleteComment(threadId, commentId, userInfo);
      await load();
    } catch {
      // silently ignore; UI unchanged so user can retry
    } finally {
      setDeletingComment(null);
    }
  }

  async function handleToggleThread(thread: DiscussionThread) {
    const userInfo = buildUserInfo();
    if (!userInfo) return;
    setTogglingThread(thread.id);
    try {
      if (thread.isClosed) {
        await discussionsApi.reopenThread(thread.id, userInfo);
      } else {
        await discussionsApi.closeThread(thread.id, userInfo);
      }
      await load();
    } catch {
      // permission error or network; leave UI unchanged
    } finally {
      setTogglingThread(null);
    }
  }

  if (loading) {
    return (
      <div className="animate-pulse space-y-3 mt-6">
        {[1, 2].map(i => (
          <div key={i} className="bg-white rounded-xl border border-gray-100 p-4 space-y-2">
            <div className="h-4 bg-gray-100 rounded w-1/2" />
            <div className="h-3 bg-gray-100 rounded w-1/4" />
          </div>
        ))}
      </div>
    );
  }

  if (error) {
    return <p className="mt-6 text-sm text-red-600">{error}</p>;
  }

  return (
    <div className="mt-8">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-base font-semibold text-gray-700">
          Discussions
          {threads.length > 0 && (
            <span className="ml-2 text-xs font-normal text-gray-400">{threads.length} thread{threads.length !== 1 ? 's' : ''}</span>
          )}
        </h2>
        {canWrite && (
          <button
            onClick={() => setShowNewThread(v => !v)}
            className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 transition-colors"
          >
            {showNewThread ? 'Cancel' : '+ New thread'}
          </button>
        )}
      </div>

      {showNewThread && (
        <div className="bg-white rounded-xl border border-indigo-100 shadow-sm p-4 mb-4">
          <input
            type="text"
            placeholder="Thread title…"
            value={newThreadTitle}
            onChange={e => setNewThreadTitle(e.target.value)}
            onKeyDown={e => e.key === 'Enter' && handleCreateThread()}
            className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300 mb-3"
            maxLength={150}
            autoFocus
          />
          <button
            onClick={handleCreateThread}
            disabled={newThreadSubmitting || !newThreadTitle.trim()}
            className="text-sm px-4 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
          >
            {newThreadSubmitting ? 'Creating…' : 'Create'}
          </button>
        </div>
      )}

      {threads.length === 0 ? (
        <div className="text-center py-10 text-gray-400 text-sm bg-white rounded-xl border border-gray-100">
          No discussions yet. Be the first to start one!
        </div>
      ) : (
        <div className="space-y-3">
          {threads.map(thread => (
            <div key={thread.id} className={`bg-white rounded-xl border shadow-sm overflow-hidden ${thread.isClosed ? 'border-amber-100' : 'border-gray-100'}`}>
              <div className="flex items-center px-5 py-4 gap-3">
                <button
                  onClick={() => setExpandedThread(expandedThread === thread.id ? null : thread.id)}
                  className="flex-1 flex items-center gap-3 text-left hover:opacity-80 transition-opacity min-w-0"
                >
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <p className={`font-medium truncate ${thread.isClosed ? 'text-gray-400' : 'text-gray-900'}`}>{thread.title}</p>
                      {thread.isClosed && <span className="shrink-0 text-xs px-1.5 py-0.5 rounded bg-amber-50 text-amber-600 font-medium">Closed</span>}
                    </div>
                    <p className="text-xs text-gray-400 mt-0.5">
                      by {thread.createdBy.userName} · {thread.comments.length} comment{thread.comments.length !== 1 ? 's' : ''}
                    </p>
                  </div>
                  <svg
                    className={`w-4 h-4 text-gray-400 shrink-0 transition-transform ${expandedThread === thread.id ? 'rotate-180' : ''}`}
                    fill="none" viewBox="0 0 24 24" stroke="currentColor"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                {isTeacher && (
                  <button
                    onClick={() => handleToggleThread(thread)}
                    disabled={togglingThread === thread.id}
                    className={`shrink-0 text-xs px-2.5 py-1 rounded-lg border transition-colors disabled:opacity-50 ${thread.isClosed ? 'border-green-200 text-green-600 hover:bg-green-50' : 'border-amber-200 text-amber-600 hover:bg-amber-50'}`}
                  >
                    {togglingThread === thread.id ? '…' : thread.isClosed ? 'Reopen' : 'Close'}
                  </button>
                )}
              </div>

              {expandedThread === thread.id && (
                <div className="border-t border-gray-100 px-5 py-4 space-y-4">
                  {thread.comments.length === 0 ? (
                    <p className="text-sm text-gray-400 italic">No comments yet.</p>
                  ) : (
                    <div className="space-y-3">
                      {thread.comments.map(comment => {
                        const isOwn = comment.author.userId === email;
                        const isEditing = editingComment?.commentId === comment.id;
                        return (
                          <div key={comment.id} className="flex gap-3">
                            <div className="w-7 h-7 rounded-full bg-indigo-100 text-indigo-600 text-xs flex items-center justify-center shrink-0 font-semibold">
                              {comment.author.userName.charAt(0).toUpperCase()}
                            </div>
                            <div className="flex-1">
                              <div className="flex items-center justify-between">
                                <p className="text-xs font-medium text-gray-700">
                                  {comment.author.userName}
                                  <span className="ml-1 font-normal text-gray-400">· {comment.author.role.name}</span>
                                </p>
                                {!thread.isClosed && (isOwn || isTeacher) && (
                                  <div className="flex gap-2">
                                    {isOwn && (
                                      <button
                                        onClick={() => setEditingComment({ threadId: thread.id, commentId: comment.id, text: comment.content })}
                                        className="text-xs text-indigo-500 hover:text-indigo-700"
                                      >
                                        Edit
                                      </button>
                                    )}
                                    <button
                                      onClick={() => handleDeleteComment(thread.id, comment.id)}
                                      disabled={deletingComment === comment.id}
                                      className="text-xs text-red-400 hover:text-red-600 disabled:opacity-50"
                                    >
                                      {deletingComment === comment.id ? '…' : 'Delete'}
                                    </button>
                                  </div>
                                )}
                              </div>
                              {isEditing ? (
                                <div className="mt-1 flex gap-2">
                                  <input
                                    type="text"
                                    value={editingComment.text}
                                    onChange={e => setEditingComment(prev => prev ? { ...prev, text: e.target.value } : prev)}
                                    onKeyDown={e => { if (e.key === 'Enter') handleEditComment(); if (e.key === 'Escape') setEditingComment(null); }}
                                    className="flex-1 text-sm border border-indigo-300 rounded-lg px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                                    maxLength={500}
                                    autoFocus
                                  />
                                  <button
                                    onClick={handleEditComment}
                                    disabled={editSubmitting || !editingComment.text.trim()}
                                    className="text-sm px-3 py-1.5 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50"
                                  >
                                    {editSubmitting ? '…' : 'Save'}
                                  </button>
                                  <button
                                    onClick={() => setEditingComment(null)}
                                    className="text-sm px-3 py-1.5 rounded-lg border border-gray-200 text-gray-500 hover:bg-gray-50"
                                  >
                                    Cancel
                                  </button>
                                </div>
                              ) : (
                                <p className="text-sm text-gray-700 mt-0.5 leading-relaxed">{comment.content}</p>
                              )}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}

                  {canWrite && !thread.isClosed && (
                    <div className="pt-2 border-t border-gray-50 space-y-1">
                      <div className="flex gap-2">
                        <input
                          type="text"
                          placeholder="Add a comment…"
                          value={commentText[thread.id] ?? ''}
                          onChange={e => { setCommentError(null); setCommentText(prev => ({ ...prev, [thread.id]: e.target.value })); }}
                          onKeyDown={e => e.key === 'Enter' && handleAddComment(thread.id)}
                          className="flex-1 text-sm border border-gray-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-300"
                          maxLength={500}
                          disabled={commentSubmitting === thread.id}
                        />
                        <button
                          onClick={() => handleAddComment(thread.id)}
                          disabled={commentSubmitting === thread.id || !commentText[thread.id]?.trim()}
                          className="text-sm px-4 py-2 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-colors"
                        >
                          {commentSubmitting === thread.id ? '…' : 'Send'}
                        </button>
                      </div>
                      {commentError && <p className="text-xs text-red-600">{commentError}</p>}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}