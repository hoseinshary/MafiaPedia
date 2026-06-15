import apiClient from './apiClient'
import type { StatisticsHomeDto } from '@/types'
export type { StatisticsHomeDto }

export interface StatisticsFilterDto {
  clubId?: number
  eventId?: number
  scenarioId?: number
}

export interface SideWinRateDto {
  id: number
  name: string
  totalGames: number
  mafiaWinRate: number
  citizenWinRate: number
}

export interface MonthlyWinRateTrendDto {
  year: number
  month: number
  totalGames: number
  mafiaWinRate: number
  citizenWinRate: number
}

export interface StatisticsDto {
  totalGames: number
  totalPlayers: number
  winRateByClub: SideWinRateDto[]
  winRateByEvent: SideWinRateDto[]
  winRateByScenario: SideWinRateDto[]
  winRateTrend: MonthlyWinRateTrendDto[]
}

export const getStatistics = (filter: StatisticsFilterDto = {}) =>
  apiClient.get<StatisticsDto>('/statistics', { params: filter })

export const getStatisticsHome = () =>
  apiClient.get<StatisticsHomeDto>('/statistics/home')
