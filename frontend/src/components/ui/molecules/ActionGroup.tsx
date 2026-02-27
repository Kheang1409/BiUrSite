import React, { ReactNode } from "react";
import { Button } from "../atoms/Button";

interface ActionGroupProps {
  actions: {
    label: string;
    icon?: string;
    onClick: () => void;
    variant?: "primary" | "secondary" | "danger" | "ghost";
    isLoading?: boolean;
  }[];
  fullWidth?: boolean;
  direction?: "row" | "column";
}

export function ActionGroup({
  actions,
  fullWidth = false,
  direction = "row",
}: ActionGroupProps) {
  return (
    <div
      className={`flex gap-2 ${
        direction === "column" ? "flex-col" : "flex-row"
      } ${fullWidth ? "w-full" : ""}`}
    >
      {actions.map((action, idx) => (
        <Button
          key={idx}
          variant={action.variant || "secondary"}
          size="sm"
          isLoading={action.isLoading}
          onClick={action.onClick}
          fullWidth={fullWidth}
          className={fullWidth && direction === "row" ? "flex-1" : ""}
        >
          {action.icon && <span className="mr-1">{action.icon}</span>}
          {action.label}
        </Button>
      ))}
    </div>
  );
}
