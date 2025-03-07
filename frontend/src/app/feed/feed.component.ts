import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PostComponent } from '../post/post.component';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, PostComponent],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css'
})
export class FeedComponent {
  posts = [
    {
    userAvatar: '(link unavailable)',
    userName: 'John Doe',
    timestamp: '2 hours ago',
    content: 'Feeling excited about the new features on BiUrSite! 🎉'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'Jane Smith',
    timestamp: '5 hours ago',
    content: 'Looking for advice on how to balance work and life. Any tips?'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'Emily Johnson',
    timestamp: '1 hour ago',
    content: 'Just finished a great project at work! Feeling accomplished 🙌'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'Michael Brown',
    timestamp: '3 hours ago',
    content: 'Does anyone have recommendations for good books to read?'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'Sarah Taylor',
    timestamp: '2 days ago',
    content: 'Excited for the weekend! Anyone have fun plans?'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'David Lee',
    timestamp: '1 day ago',
    content: 'Just learned about a new feature on BiUrSite. Has anyone else tried it?'
    },
    {
    userAvatar: '(link unavailable)',
    userName: 'Olivia Martin',
    timestamp: '4 hours ago',
    content: 'Looking for a new coffee shop to try. Any recommendations?'
    }
  ];
}
