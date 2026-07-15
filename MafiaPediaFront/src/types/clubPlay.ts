export interface ReplaceParticipantDto {
  newClubPlayerId: number
  isGuest: boolean
}

export interface MasterContextDto {
  masterId: number
  masterName: string
  clubId: number
  clubName: string
}

export interface ParticipantInputDto {
  clubPlayerId: number
  isGuest: boolean
}

export interface CreateClubPlayDto {
  title?: string
  dateTime: string
  roomId: number
  senarioId: number
  playersCount: number
  desc?: string
  link?: string
  playType: 'normal' | 'rank' | 'superrank' | 'etc'
  eventId?: number | null
  shuffleRoles?: boolean
  masterId?: number
  participants: ParticipantInputDto[]
}

export interface ClubPlayParticipantDto {
  clubPlayerId: number
  name: string
  roleId: number
  roleName: string
  sideId: number
  rolePhoto: string | null
  isGuest: boolean
}

export interface ClubPlayDetailDto {
  id: number
  title: string | null
  dateTime: string
  roomId: number
  roomName: string
  senarioId: number
  senarioName: string
  playersCount: number
  guestCount: number
  desc: string | null
  link: string | null
  playType: string
  status: string
  masterId: number
  masterName: string
  winnersideId: number | null
  eventId: number
  eventName: string
  participants: ClubPlayParticipantDto[]
}

export interface ClubPlayListItemDto {
  id: number
  title: string | null
  dateTime: string
  businessDate: string
  roomName: string
  senarioName: string
  playersCount: number
  guestCount: number
  status: string
  playType: string
  masterName?: string | null
}

export interface MasterStatsDto {
  totalPlays: number
  totalEntries: number
  totalGuestEntries: number
}

export interface MasterPerformanceDto {
  masterId: number
  masterName: string
  playCount: number
  entryCount: number
  guestEntryCount: number
}

export interface EventDto {
  id: number
  name: string
  clubId: number
  isDefault: boolean
}
