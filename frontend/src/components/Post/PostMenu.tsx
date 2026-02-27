"use client";

import React, { memo, useCallback } from "react";
import { UseDropdownMenuResult } from "@/hooks/useDropdownMenu";

interface PostMenuProps {
  menu: UseDropdownMenuResult;
  isSaving?: boolean;
  onEditClick: () => void;
  onDeleteClick: () => void;
}

export const PostMenu = memo(function PostMenu({
  menu,
  isSaving = false,
  onEditClick,
  onDeleteClick,
}: PostMenuProps) {
  const handleEditClick = useCallback(() => {
    menu.close();
    onEditClick();
  }, [menu, onEditClick]);

  const handleDeleteClick = useCallback(() => {
    menu.close();
    onDeleteClick();
  }, [menu, onDeleteClick]);

  return (
    <div className="relative" ref={menu.menuRef}>
      <button
        type="button"
        onClick={menu.toggle}
        className="w-9 h-9 rounded-full grid place-items-center bg-transparent hover:bg-white/10 transition-colors text-muted hover:text-white focus:outline-none focus-visible:ring-2 focus-visible:ring-white/20"
        aria-label="Post options"
        title="Options"
        disabled={isSaving}
      >
        <svg
          width="18"
          height="18"
          viewBox="0 0 24 24"
          fill="currentColor"
          aria-hidden="true"
        >
          <circle cx="5" cy="12" r="2" />
          <circle cx="12" cy="12" r="2" />
          <circle cx="19" cy="12" r="2" />
        </svg>
      </button>

      {menu.isOpen && (
        <div className="absolute right-0 mt-2 w-48 rounded-xl border border-white/10 bg-primary-3 shadow-2xl overflow-hidden z-10">
          <button
            type="button"
            className="w-full text-left px-3 py-2.5 text-sm text-white hover:bg-white/10 transition-colors"
            onClick={handleEditClick}
          >
            Edit post
          </button>
          <div className="h-px bg-white/10" />
          <button
            type="button"
            className="w-full text-left px-3 py-2.5 text-sm text-danger-1 hover:bg-white/10 transition-colors"
            onClick={handleDeleteClick}
          >
            Delete post
          </button>
        </div>
      )}
    </div>
  );
});
