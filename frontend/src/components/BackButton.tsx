import { Button } from '@mui/material';
import { ArrowBack } from '@mui/icons-material';

function BackButton({ label = 'Geri', onClick }: { label?: string; onClick: () => void }) {
    return (
        <Button startIcon={<ArrowBack />} onClick={onClick} sx={{ mb: 2 }}>
            {label}
        </Button>
    );
}

export default BackButton;
