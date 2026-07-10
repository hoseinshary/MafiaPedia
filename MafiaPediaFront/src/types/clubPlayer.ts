export interface ClubPlayerDto {
  id: number
  name: string
  mobile: string
  birthday: string | null
  code: string | null
  picture: string | null
  desc: string | null
  joinedAt: string | null
}

export interface ClubPlayerJoinResult {
  clubPlayer: ClubPlayerDto
  wasExistingCustomer: boolean
}

export interface CustomerSearchResultDto {
  inClub: ClubPlayerDto[]
  globalOthers: ClubPlayerDto[]
}
