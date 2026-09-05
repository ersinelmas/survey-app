import { createTheme } from '@mui/material/styles';

export const fontFamilyMono = "'IBM Plex Mono', 'Consolas', monospace";

const ink = '#22314F';
const paper = '#F7F8FA';
const stamp = '#0F7A6C';

const theme = createTheme({
    palette: {
        primary: {
            main: ink,
        },
        secondary: {
            main: stamp,
        },
        background: {
            default: paper,
            paper: '#FFFFFF',
        },
        divider: '#DDE2E8',
    },
    shape: {
        borderRadius: 6,
    },
    typography: {
        fontFamily: "'Inter', 'Roboto', sans-serif",
        h1: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 700 },
        h2: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 700 },
        h3: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 600 },
        h4: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 600 },
        h5: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 600 },
        h6: { fontFamily: "'Space Grotesk', sans-serif", fontWeight: 600 },
    },
    components: {
        MuiAppBar: {
            styleOverrides: {
                root: {
                    backgroundColor: ink,
                    boxShadow: 'none',
                    border: 'none',
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {
                    textTransform: 'none',
                    fontWeight: 500,
                },
            },
        },
        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: 'none',
                    border: '1px solid #DDE2E8',
                },
            },
            defaultProps: {
                elevation: 0,
            },
        },
        MuiTableHead: {
            styleOverrides: {
                root: {
                    backgroundColor: '#F0F2F5',
                    '& .MuiTableCell-root': {
                        fontFamily: "'Space Grotesk', sans-serif",
                        fontWeight: 600,
                        fontSize: '0.8125rem',
                        letterSpacing: '0.02em',
                        textTransform: 'uppercase',
                        color: ink,
                    },
                },
            },
        },
        MuiChip: {
            styleOverrides: {
                root: {
                    fontWeight: 500,
                },
            },
        },
        MuiTableBody: {
            styleOverrides: {
                root: {
                    '& .MuiTableRow-root:hover': {
                        backgroundColor: '#F7F8FA',
                    },
                },
            },
        },
    },
});

export default theme;
