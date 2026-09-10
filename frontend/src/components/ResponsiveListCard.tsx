import { Card, CardContent, Box, Typography } from '@mui/material';
import type { ReactNode } from 'react';

interface ResponsiveListCardField {
    label: string;
    value: ReactNode;
}

interface ResponsiveListCardProps {
    title: ReactNode;
    fields?: ResponsiveListCardField[];
    actions?: ReactNode;
}

function ResponsiveListCard({ title, fields, actions }: ResponsiveListCardProps) {
    return (
        <Card sx={{ mb: 1.5 }}>
            <CardContent sx={{ '&:last-child': { pb: 2 } }}>
                <Typography variant="subtitle1" sx={{ mb: fields?.length ? 1 : 0 }}>
                    {title}
                </Typography>
                {fields?.map((field, index) => (
                    <Box
                        key={index}
                        sx={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'flex-start',
                            gap: 2,
                            py: 0.75,
                            borderTop: '1px solid',
                            borderColor: 'divider',
                        }}
                    >
                        <Typography variant="caption" color="text.secondary" sx={{ flexShrink: 0, pt: 0.25 }}>
                            {field.label}
                        </Typography>
                        <Box sx={{ textAlign: 'right', minWidth: 0 }}>{field.value}</Box>
                    </Box>
                ))}
                {actions && (
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5, mt: 1, borderTop: '1px solid', borderColor: 'divider', pt: 1 }}>
                        {actions}
                    </Box>
                )}
            </CardContent>
        </Card>
    );
}

export default ResponsiveListCard;
