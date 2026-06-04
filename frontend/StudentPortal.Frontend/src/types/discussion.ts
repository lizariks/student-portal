export interface UserRole {
  name: string;
}

export interface UserInfo {
  userId: string;
  userName: string;
  role: UserRole;
}

export interface Comment {
  id: string;
  parentCommentId?: string;
  author: UserInfo;
  content: string;
  isResolved: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface DiscussionThread {
  id: string;
  targetId: string;
  targetType: number;
  title: string;
  createdBy: UserInfo;
  isClosed: boolean;
  comments: Comment[];
  createdAt: string;
  updatedAt: string;
}