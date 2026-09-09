import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { SnackbarProvider } from './context/SnackbarContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import Dashboard from './pages/Dashboard';
import UserDashboard from './pages/UserDashboard';
import AnswerTemplatesPage from './pages/AnswerTemplatesPage';
import QuestionsPage from './pages/QuestionsPage';
import SurveysPage from './pages/SurveysPage';
import FillSurveyPage from './pages/FillSurveyPage';
import SurveyReportPage from './pages/SurveyReportPage';
import PrivacyPolicyPage from './pages/PrivacyPolicyPage';

function App() {
  return (
    <BrowserRouter>
      <SnackbarProvider>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/privacy" element={<PrivacyPolicyPage />} />
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <Navigate to="/dashboard" replace />
                </ProtectedRoute>
              }
            />
            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Dashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/answer-templates"
              element={
                <ProtectedRoute>
                  <AnswerTemplatesPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/questions"
              element={
                <ProtectedRoute>
                  <QuestionsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/surveys"
              element={
                <ProtectedRoute>
                  <SurveysPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/surveys/:surveyId/report"
              element={
                <ProtectedRoute>
                  <SurveyReportPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="/my-surveys"
              element={
                <ProtectedRoute>
                  <UserDashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/my-surveys/:surveyId"
              element={
                <ProtectedRoute>
                  <FillSurveyPage />
                </ProtectedRoute>
              }
            />
          </Routes>
        </AuthProvider>
      </SnackbarProvider>
    </BrowserRouter>
  );
}

export default App;
