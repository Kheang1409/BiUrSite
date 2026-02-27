import React, { InputHTMLAttributes, forwardRef } from "react";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  fullWidth?: boolean;
  helpText?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  (
    { label, error, fullWidth = true, helpText, className = "", ...props },
    ref
  ) => {
    return (
      <div className={fullWidth ? "w-full" : ""}>
        {label && (
          <label className="block text-sm font-medium text-white mb-2">
            {label}
          </label>
        )}
        <input
          ref={ref}
          className={[
            "px-4 py-2.5 bg-white/10 border border-white/20 rounded-[10px] text-white placeholder-muted",
            "focus:outline-none focus:border-primary-1 focus:ring-2 focus:ring-primary-1/50",
            "transition-colors duration-200",
            error && "border-danger-1 focus:ring-danger-1/50",
            fullWidth && "w-full",
            className,
          ]
            .filter(Boolean)
            .join(" ")}
          {...props}
        />
        {error && <p className="text-danger-1 text-xs mt-1">{error}</p>}
        {helpText && !error && (
          <p className="text-muted text-xs mt-1">{helpText}</p>
        )}
      </div>
    );
  }
);

Input.displayName = "Input";
