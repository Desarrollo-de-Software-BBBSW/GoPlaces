import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'relativeTime',
  standalone: true,
})
export class RelativeTimePipe implements PipeTransform {
  transform(value?: string | Date): string {
    if (!value) return '';

    const date = value instanceof Date ? value : new Date(value);
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (seconds < 60) return 'hace unos segundos';

    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `hace ${minutes} ${minutes === 1 ? 'minuto' : 'minutos'}`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `hace ${hours} ${hours === 1 ? 'hora' : 'horas'}`;

    const days = Math.floor(hours / 24);
    if (days < 30) return `hace ${days} ${days === 1 ? 'día' : 'días'}`;

    const months = Math.floor(days / 30);
    if (months < 12) return `hace ${months} ${months === 1 ? 'mes' : 'meses'}`;

    const years = Math.floor(months / 12);
    return `hace ${years} ${years === 1 ? 'año' : 'años'}`;
  }
}
