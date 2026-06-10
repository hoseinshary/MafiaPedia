export interface OverallRankingEntry {
  playerId: number
  playerName: string
  totalGames: number
  overallWinRate: number
  citizenWinRate: number
  mafiaWinRate: number
}

export interface SideRankingEntry {
  playerId: number
  playerName: string
  games: number
  wins: number
  winRate: number
}

export interface PlayerStatistics {
  totalGames: number
  overallWinRate: number
  citizenGames: number
  citizenWinRate: number
  mafiaGames: number
  mafiaWinRate: number
}

export interface RoleStat {
  roleId: number
  roleName: string
  games: number
  wins: number | null
  winRate: number | null
}

export interface RecentGame {
  playId: number
  playTitle: string
  roleName: string
  result: string
  link: string | null
}

export interface PlayerProfile {
  id: number
  name: string
  picture: string | null
  birthday: string | null
  statistics: PlayerStatistics
  mostPlayedRoles: RoleStat[]
  bestRoles: RoleStat[]
  recentGames: RecentGame[]
}

export interface Club {
  id: number
  name: string
}

export interface Event {
  id: number
  name: string
  clubId: number
}

export interface Scenario {
  id: number
  name: string
}

export interface Role {
  id: number
  name: string
  senarioId: number
}

export interface PlayerSearchResult {
  id: number
  name: string
  totalGames: number
  picture: string | null
}

export interface PlayPlayerInput {
  playerId: number
  roleId: number
  action: number
  rank: number
}

export interface CreatePlayPayload {
  title: string
  dateTime: string
  playersCount: number
  desc: string
  senarioId: number
  winnersideId: number
  eventId: number
  roomId: number
  masterId: number
  guestCount: number
  link: string
  players: PlayPlayerInput[]
}

export interface CommentDto {
  id: number
  content: string
  userDisplayName: string
  userId: number
  parentCommentId: number | null
  createdAt: string
  likeCount: number
  isLikedByCurrentUser: boolean
  replies: CommentDto[]
}

export interface SideRankingParams {
  sideId: number
  clubId?: number
  eventId?: number
  scenarioId?: number
  minimumGames?: number
}

export interface PlayDto {
  id: number
  title: string
  dateTime: string
  playersCount: number
  guestCount: number
  desc: string | null
  link: string | null
  senarioId: number
  senarioName: string
  winnersideId: number
  winnersideName: string
  eventId: number
  eventName: string
  roomId: number
  roomName: string
  masterId: number
  masterName: string
  userId?: number
  players?: PlayPlayerInput[]
}

export interface PaginatedPlays {
  items: PlayDto[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

export interface DropdownMaster {
  id: number
  name: string
}

export interface DropdownRoom {
  id: number
  name: string
}

export interface DropdownSide {
  id: number
  name: string
}

export interface DropdownData {
  senarios: Scenario[]
  masters: DropdownMaster[]
  events: Event[]
  rooms: DropdownRoom[]
  sides: DropdownSide[]
  roles: Role[]
}

export interface PlayerListItem {
  id: number
  name: string
  code: string
  picture: string | null
  totalGames: number
}

export interface PaginatedPlayers {
  items: PlayerListItem[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

export interface PlayerDetail {
  id: number
  name: string
  code: string
  mobile: string
  birthday: string | null
  picture: string | null
  desc: string | null
}

export interface UserDto {
  id: number
  username: string
  displayName: string
  mobile: string
  role: string
  isActive: boolean
  lastLogin: string | null
  createdAt: string
}

export interface PaginatedUsers {
  items: UserDto[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CreateUserDto {
  username: string
  password: string
  displayName?: string
  mobile?: string
  role: string
}

export interface UpdateUserDto {
  displayName?: string
  mobile?: string
  role?: string
  isActive?: boolean
  password?: string
}
