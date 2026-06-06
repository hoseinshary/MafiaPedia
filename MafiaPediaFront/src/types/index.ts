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
  scenarioId: number
}

export interface SideRankingParams {
  sideId: number
  clubId?: number
  eventId?: number
  scenarioId?: number
  minimumGames?: number
}
