import dayjs from "dayjs";

export function formatFacebookDate(dateString: string): {
  display: string;
  fullDate: string;
} {
  const date = dayjs(dateString);
  const now = dayjs();
  const diffInHours = now.diff(date, "hour");
  const diffInDays = now.diff(date, "day");
  const isSameYear = date.year() === now.year();
  const month = date.format("MMMM");
  const dayNum = date.format("D");
  const year = date.format("YYYY");
  const time = date.format("h:mm A");

  let display: string;

  // Less than 24 hours
  if (diffInHours < 24) {
    if (diffInHours < 1) {
      display = `Today at ${time}`;
    } else {
      display = `Today at ${time}`;
    }
  }
  // Yesterday (24-48 hours)
  else if (diffInDays === 1) {
    display = `Yesterday at ${time}`;
  }
  // Less than 7 days, same year
  else if (diffInDays < 7 && isSameYear) {
    display = `${month} ${dayNum} at ${time}`;
  }
  // More than 7 days, same year
  else if (isSameYear) {
    display = `${month} ${dayNum} at ${time}`;
  }
  // Different year
  else {
    display = `${month} ${dayNum}, ${year} at ${time}`;
  }

  // Full date for tooltip
  const fullDate = date.format("MMMM D, YYYY [at] h:mm A");

  return { display, fullDate };
}

export function formatAgeShort(dateString: string): {
  display: string;
  fullDate: string;
} {
  const date = dayjs(dateString);
  const now = dayjs();

  const diffInDays = Math.max(0, now.diff(date, "day"));

  let display: string;
  if (diffInDays < 7) {
    display = `${diffInDays}d`;
  } else if (diffInDays < 28) {
    const weeks = Math.max(1, Math.floor(diffInDays / 7));
    display = `${weeks}w`;
  } else if (diffInDays < 365) {
    const months = Math.max(1, Math.floor(diffInDays / 28));
    display = `${months}m`;
  } else {
    const years = Math.max(1, Math.floor(diffInDays / 365));
    display = `${years}y`;
  }

  const fullDate = date.format("dddd, MMMM D, YYYY [at] h:mm A");
  return { display, fullDate };
}
