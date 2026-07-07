import { useState } from "react";
import { callApi } from "./services/apiService";
import "./App.css";

function App() {
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState("");
  const [error, setError] = useState("");

  const handleCallApi = async () => {
    setLoading(true);
    setError("");
    setResult("");

    try {
      const data = await callApi(
        "https://jsonplaceholder.typicode.com/posts/1",
        {
          userId: 1,
        },
      );
      setResult(JSON.stringify(data, null, 2));
    } catch (err) {
      setError(err.message || "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container">
      <h1>API Service Demo</h1>
      <p>Pass the API URL and params into the reusable service.</p>
      <button type="button" onClick={handleCallApi} disabled={loading}>
        {loading ? "Loading..." : "Call API"}
      </button>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {result && (
        <pre style={{ whiteSpace: "pre-wrap", marginTop: "1rem" }}>
          {result}
        </pre>
      )}
    </div>
  );
}

export default App;
