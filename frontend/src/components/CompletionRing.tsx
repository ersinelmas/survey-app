import { fontFamilyMono } from '../theme';

function CompletionRing({ percent, size = 120 }: { percent: number; size?: number }) {
    const strokeWidth = size * 0.09;
    const radius = (size - strokeWidth) / 2;
    const circumference = 2 * Math.PI * radius;
    const offset = circumference * (1 - Math.min(Math.max(percent, 0), 100) / 100);
    const center = size / 2;

    return (
        <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} role="img" aria-label={`Tamamlanma oranı yüzde ${percent}`}>
            <circle cx={center} cy={center} r={radius} stroke="#DDE2E8" strokeWidth={strokeWidth} fill="none" />
            <circle
                cx={center}
                cy={center}
                r={radius}
                stroke="#0F7A6C"
                strokeWidth={strokeWidth}
                fill="none"
                strokeDasharray={circumference}
                strokeDashoffset={offset}
                strokeLinecap="round"
                transform={`rotate(-90 ${center} ${center})`}
                style={{ transition: 'stroke-dashoffset 0.4s ease' }}
            />
            <text
                x={center}
                y={center}
                textAnchor="middle"
                dominantBaseline="central"
                fontFamily={fontFamilyMono}
                fontSize={size * 0.2}
                fontWeight={600}
                fill="#22314F"
            >
                {percent}%
            </text>
        </svg>
    );
}

export default CompletionRing;
