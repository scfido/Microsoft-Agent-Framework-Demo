import { createFileRoute } from '@tanstack/react-router'
import { HttpAgent } from "@ag-ui/client";

export const Route = createFileRoute('/')({ component: App })


const myAgent = new HttpAgent({
    url: "http://localhost:5050",
  });

function App() {

    const { agent } = useAgent({ agentId: "AGUIAssistant" });
    return (
        <div>
            <p>Agent ID: {agent.agentId}</p>
            <p>Thread ID: {agent.threadId}</p>
            <p>Status: {agent.isRunning ? "Running" : "Idle"}</p>
            <p>Messages: {agent.messages.length}</p>
        </div>
    );
}
