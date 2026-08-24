import { createTheme } from '@mui/material/styles';

const theme = createTheme({
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    textTransform: 'none',
                },
            },
        },
        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: 'none',
                },
            },
            defaultProps: {
                elevation: 2,
            },
        },
        MuiTableHead: {
            styleOverrides: {
                root: {
                    backgroundColor: '#f4f6f8',
                },
            },
        },
    },
    palette: {
        background: {
            default: '#f4f6f8',
        },
    },
});

export default theme;