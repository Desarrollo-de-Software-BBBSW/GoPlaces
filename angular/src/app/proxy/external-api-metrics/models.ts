
export interface ApiUsageMetricDto {
  apiName?: string;
  totalCalls: number;
  successfulCalls: number;
  failedCalls: number;
  averageDurationMs: number;
}
