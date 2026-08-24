import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import AdminDashboard from './pages/AdminDashboard';
import UserDashboard from './pages/UserDashboard';
import AnswerTemplatesPage from './pages/AnswerTemplatesPage';
import QuestionsPage from './pages/QuestionsPage';
import SurveysPage from './pages/SurveysPage';
import FillSurveyPage from './pages/FillSurveyPage';
import SurveyReportPage from './pages/SurveyReportPage';

function HomeRedirect() {
  const { role } = useAuth();
  if (role === 'Admin') return <Navigate to="/admin" replace />;
  return <Navigate to="/my-surveys" replace />;
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <HomeRedirect />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin"
            element={
              <ProtectedRoute requiredRole="Admin">
                <AdminDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/answer-templates"
            element={
              <ProtectedRoute requiredRole="Admin">
                <AnswerTemplatesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/questions"
            element={
              <ProtectedRoute requiredRole="Admin">
                <QuestionsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/admin/surveys"
            element={
              <ProtectedRoute requiredRole="Admin">
                <SurveysPage />
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
          <Route
            path="/admin/surveys/:surveyId/report"
            element={
              <ProtectedRoute requiredRole="Admin">
                <SurveyReportPage />
              </ProtectedRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;