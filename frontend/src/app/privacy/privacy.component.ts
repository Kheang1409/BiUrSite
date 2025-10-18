import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './privacy.component.html',
  styleUrls: ['./privacy.component.css'],
})
export class PrivacyComponent {
  isSectionOpen: Record<'info' | 'usage' | 'security' | 'contact', boolean> = {
    info: true,
    usage: true,
    security: false,
    contact: true,
  };

  toggleSection(section: keyof typeof this.isSectionOpen) {
    this.isSectionOpen[section] = !this.isSectionOpen[section];
  }
}
