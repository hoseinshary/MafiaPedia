export interface ClubDto {
  id: number
  name: string
  address: string | null
  phone: string | null
  city: string | null
  description: string | null
  logo: string | null
}

export interface RoomDto {
  id: number
  name: string
  clubId: number
  isActive: boolean
}

export interface MasterDto {
  id: number
  name: string
  clubId: number
  userId: number | null
  userDisplayName: string | null
  userMobile: string | null
  ratePerGame: number | null
  photo: string | null
  bio: string | null
}

export interface ClubDetailDto {
  id: number
  name: string
  address: string | null
  phone: string | null
  city: string | null
  description: string | null
  logo: string | null
  rooms: RoomDto[]
  masters: MasterDto[]
}

export interface CreateClubDto {
  name: string
  address?: string | null
  phone?: string | null
  city?: string | null
  description?: string | null
}

export interface UpdateClubDto {
  name?: string
  address?: string | null
  phone?: string | null
  city?: string | null
  description?: string | null
}

export interface CreateRoomDto {
  name: string
  isActive?: boolean
}

export interface UpdateRoomDto {
  name?: string
  isActive?: boolean
}

export interface CreateMasterDto {
  name: string
  ratePerGame?: number | null
  existingUserId?: number | null
  newUser?: NewMasterUserDto | null
}

export interface NewMasterUserDto {
  username: string
  password: string
  mobile: string
  displayName?: string | null
}

export interface UpdateMasterDto {
  name?: string
  ratePerGame?: number | null
  existingUserId?: number | null
  unlinkUser?: boolean
}

export interface ClubListItem {
  id: number
  name: string
  roomCount?: number
  masterCount?: number
}

export interface ClubUserDto {
  id: number
  userId: number
  userDisplayName: string | null
  userMobile: string | null
  clubuserRole: 'owner' | 'supervisor' | 'cashier' | 'master'
  clubId: number
  masterId: number | null
  masterName: string | null
}

export interface NewClubUserAccountDto {
  username: string
  password: string
  mobile: string
  displayName: string | null
}

export interface CreateClubUserDto {
  clubuserRole: 'owner' | 'supervisor' | 'cashier'
  existingUserId?: number | null
  newUser?: NewClubUserAccountDto | null
}

export interface UpdateClubUserRoleDto {
  clubuserRole: 'owner' | 'supervisor' | 'cashier' | 'master'
}

export interface ClubUserContextDto {
  clubId: number
  clubName: string
  clubuserRole: string
}


