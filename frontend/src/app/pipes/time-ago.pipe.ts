import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeAgo'
})
export class TimeAgoPipe implements PipeTransform {

  transform(value: string | Date): string {
    if (!value) return '';

    const date = new Date(typeof value === 'string' ? value + 'Z' : value);
    const now = new Date();

    const seconds = Math.abs(Math.floor((now.getTime() - date.getTime()) / 1000));

    if (seconds < 60) {
      return 'Just now';
    }

    const intervals = {
      year: 31536000,
      month: 2592000,
      week: 604800,
      day: 86400,
      hour: 3600,
      minute: 60
    };

    let counter;
    for (const [unit, secondsInUnit] of Object.entries(intervals)) {
      counter = Math.floor(seconds / secondsInUnit);
      if (counter > 0) {
        if (counter === 1) {
          return `${counter} ${unit} ago`;
        } else {
          return `${counter} ${unit}s ago`;
        }
      }
    }

    return date.toLocaleString('en-US', { month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }
}