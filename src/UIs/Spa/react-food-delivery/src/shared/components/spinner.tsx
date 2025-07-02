// @components/Spinner.tsx
import type { CSSProperties } from "react";
import { PulseLoader } from "react-spinners";

type SpinnerProps = {
  size?: number;
  color?: string;
  className?: string;
  fullScreen?: boolean;
};

export const Spinner = ({
  size = 12,
  color = "#3b82f6",
  className = "",
  fullScreen = false,
}: SpinnerProps) => {
  const override: CSSProperties = {
    display: "block",
    margin: "0 auto",
  };

  const spinner = (
    <PulseLoader
      color={color}
      loading={true}
      cssOverride={override}
      size={size}
      speedMultiplier={0.8}
    />
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 bg-black bg-opacity-30 flex items-center justify-center z-50">
        {spinner}
      </div>
    );
  }

  return (
    <div className={`flex justify-center items-center ${className}`}>
      {spinner}
    </div>
  );
};
