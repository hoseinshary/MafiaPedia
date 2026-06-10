import apiClient from './apiClient'

export const CommentApi = {
  getComments(entityType: string, entityId: number) {
    return apiClient.get(`/comments/${entityType}/${entityId}`)
  },
  addComment(entityType: string, entityId: number, content: string, parentCommentId?: number) {
    const body: { content: string; parentCommentId?: number } = { content }
    if (parentCommentId) body.parentCommentId = parentCommentId
    return apiClient.post(`/comments/${entityType}/${entityId}`, body)
  },
  deleteComment(commentId: number) {
    return apiClient.delete(`/comments/${commentId}`)
  },
  toggleLike(commentId: number) {
    return apiClient.post(`/comments/${commentId}/like`)
  },
}
