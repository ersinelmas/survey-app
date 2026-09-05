import { Box, Typography } from '@mui/material';

function EmptyState({ message }: { message: string }) {
    return (
        <Box
            sx={{
                border: '1px dashed',
                borderColor: 'divider',
                borderRadius: 1,
                p: 4,
                textAlign: 'center',
            }}
        >
            <Typography color="text.secondary">{message}</Typography>
        </Box>
    );
}

export default EmptyState;
