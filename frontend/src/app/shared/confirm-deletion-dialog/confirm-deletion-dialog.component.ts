import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirm-deletion-dialog',
  imports: [],
  templateUrl: './confirm-deletion-dialog.component.html',
  styleUrl: './confirm-deletion-dialog.component.css'
})
export class ConfirmDeletionDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDeletionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { itemType: string } 
  ) {}

  onCancel(): void {
    this.dialogRef.close(false); 
  }

  onConfirm(): void {
    this.dialogRef.close(true); 
  }
}
