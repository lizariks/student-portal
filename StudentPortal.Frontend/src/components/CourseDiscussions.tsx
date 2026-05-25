import { useState, useEffect, useCallback } from 'react';
import { discussionsApi } from '../api/discussions';
import { getCatalogUser } from '../api/users';
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

  const canWrite = roles.includes('teacher') || roles.includes('student') ||
    roles.includes('Teacher') || roles.includes('Student') ||
    roles.includes('admin') || roles.includes('Admin');

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

  async function buildUserInfo(): Promise<UserInfo | null> {
    if (!email) return null;
    const catalogUser = await getCatalogUser(email, name);
    if (!catalogUser) return null;
    const role = roles[0] ?? 'Student';
    return {
      userId: String(catalogUser.id),
      userName: `${catalogUser.firstName} ${catalogUser.lastName}`.trim() || catalogUser.nickname,
      role: { name: role },
    };
  }

  async function handleCreateThread() {
    if (!newThreadTitle.trim()) return;
    setNewThreadSubmitting(true);
    try {
      const userInfo = await buildUserInfo();
      if (!userInfo) return;
      await discussionsApi.createThread(String(courseId), 0, newThreadTitle.trim(), userInfo);
      setNewThreadTitle('');
      setShowNewThread(false);
      await load();
    } catch {
      // silently fail — user sees no change
    } finally {
      setNewThreadSubmitting(false);
    }
  }

  async function handleAddComment(threadId: string) {
    const text = commentText[threadId]?.trim();
    if (!text) return;
    setCommentSubmitting(threadId);
    try {
      const userInfo = await buildUserInfo();
      if (!userInfo) return;
      const comment: Omit<Comment, 'id' | 'createdAt' | 'updatedAt'> = {
        author: userInfo,
        content: text,
        isResolved: false,
      };
      await discussionsApi.addComment(threadId, comment);
      setCommentText(prev => ({ ...prev, [threadId]: '' }));
      await load();
    } catch {
      // silently fail
    } finally {
      setCommentSubmitting(null);
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
            <div key={thread.id} className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
              <button
                onClick={() => setExpandedThread(expandedThread === thread.id ? null : thread.id)}
                className="w-full flex items-center justify-between px-5 py-4 text-left hover:bg-gray-50 transition-colors"
              >
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-900 truncate">{thread.title}</p>
                  <p className="text-xs text-gray-400 mt-0.5">
                    by {thread.createdBy.userName} · {thread.comments.length} comment{thread.comments.length !== 1 ? 's' : ''}
                    {thread.isClosed && <span className="ml-2 text-amber-500">· Closed</span>}
                  </p>
                </div>
                <svg
                  className={`w-4 h-4 text-gray-400 shrink-0 ml-3 transition-transform ${expandedThread === thread.id ? 'rotate-180' : ''}`}
                  fill="none" viewBox="0 0 24 24" stroke="currentColor"
                >
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
              </button>

              {expandedThread === thread.id && (
                <div className="border-t border-gray-100 px-5 py-4 space-y-4">
                  {thread.comments.length === 0 ? (
                    <p className="text-sm text-gray-400 italic">No comments yet.</p>
                  ) : (
                    <div className="space-y-3">
                      {thread.comments.map(comment => (
                        <div key={comment.id} className="flex gap-3">
                          <div className="w-7 h-7 rounded-full bg-indigo-100 text-indigo-600 text-xs flex items-center justify-center shrink-0 font-semibold">
                            {comment.author.userName.charAt(0).toUpperCase()}
                          </div>
                          <div className="flex-1">
                            <p className="text-xs font-medium text-gray-700">
                              {comment.author.userName}
                              <span className="ml-1 font-normal text-gray-400">· {comment.author.role.name}</span>
                            </p>
                            <p className="text-sm text-gray-700 mt-0.5 leading-relaxed">{comment.content}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {canWrite && !thread.isClosed && (
                    <div className="flex gap-2 pt-2 border-t border-gray-50">
                      <input
                        type="text"
                        placeholder="Add a comment…"
                        value={commentText[thread.id] ?? ''}
                        onChange={e => setCommentText(prev => ({ ...prev, [thread.id]: e.target.value }))}
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