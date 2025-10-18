import {
  Component,
  EventEmitter,
  Input,
  Output,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { User } from '../../models/users/user';

@Component({
  selector: 'app-profile-left',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-left.component.html',
  styleUrls: ['./profile-left.component.css'],
})
export class ProfileLeftComponent {
  @Input() user: User = new User();
  @Input() editMode = false;
  @Input() selectedImagePreview: string | null = null;

  // Emit the raw change event so the parent can handle resizing/processing
  @Output() fileSelected = new EventEmitter<Event>();
  @Output() toggleEdit = new EventEmitter<void>();

  @ViewChild('fileInput', { static: false })
  fileInputRef!: ElementRef<HTMLInputElement>;

  // Called by parent to reset the native file input value
  clearFileInput(): void {
    if (this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.value = '';
    }
  }

  // Helper for the avatar click to open the file picker
  openFilePicker(): void {
    if (this.editMode && this.fileInputRef?.nativeElement) {
      this.fileInputRef.nativeElement.click();
    }
  }
}
