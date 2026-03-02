import React, { InputHTMLAttributes, forwardRef } from "react";

interface FormInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
}

export const FormInput = forwardRef<HTMLInputElement, FormInputProps>(
  ({ label, error, className = "", id, ...props }, ref) => {
    const inputId = id || `input-${label.toLowerCase().replace(/\s+/g, "-")}`;

    return (
      <div>
        <label
          htmlFor={inputId}
          className="block text-sm font-medium text-gray-900 dark:text-white mb-2"
        >
          {label}
        </label>
        <input
          ref={ref}
          id={inputId}
          className={`w-full px-4 py-3 rounded-lg bg-gray-100 dark:bg-white/5 border border-gray-200 dark:border-white/10 text-gray-900 dark:text-white placeholder:text-gray-400 dark:placeholder:text-white/40 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 ${
            error ? "border-red-500/50" : ""
          } ${className}`}
          {...props}
        />
        {error && <p className="mt-1 text-sm text-red-400">{error}</p>}
      </div>
    );
  },
);

FormInput.displayName = "FormInput";
