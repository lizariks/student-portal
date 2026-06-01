import client from './client';
import type { DiscussionThread, UserInfo, Comment } from '../types/discussion';

const BASE = '/discussionthread';

export const discussionsApi = {
  getByTarget: (targetId: string, targetType: number) =>
    client
      .get<DiscussionThread[]>(`${BASE}/by-target`, { params: { targetId, targetType } })
      .then(r => r.data),

  getById: (threadId: string) =>
    client.get<DiscussionThread>(`${BASE}/${threadId}`).then(r => r.data),

  createThread: (targetId: string, targetType: number, title: string, createdBy: UserInfo) =>
    client
      .post<DiscussionThread>(BASE, { targetId, targetType, title, createdBy })
      .then(r => r.data),

  addComment: (threadId: string, comment: Omit<Comment, 'id' | 'createdAt' | 'updatedAt'>) =>
    client
      .post<DiscussionThread>(`${BASE}/${threadId}/add-comment`, { threadId, comment })
      .then(r => r.data),

  editComment: (threadId: string, commentId: string, newContent: string, actor: UserInfo) =>
    client
      .put<DiscussionThread>(`${BASE}/${threadId}/edit-comment`, { threadId, commentId, newContent, actor })
      .then(r => r.data),

  deleteComment: (threadId: string, commentId: string, actor: UserInfo) =>
    client
      .delete<DiscussionThread>(`${BASE}/${threadId}/comments/${commentId}`, { data: actor })
      .then(r => r.data),

  closeThread: (threadId: string, actor: UserInfo) =>
    client.post(`${BASE}/${threadId}/close`, actor),

  reopenThread: (threadId: string, actor: UserInfo) =>
    client.post<DiscussionThread>(`${BASE}/${threadId}/reopen`, actor).then(r => r.data),
};