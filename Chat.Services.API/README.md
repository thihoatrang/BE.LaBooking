### Chat.Services.API

REST API to power an AI chatbot that advises customers about lawyers and services in this system.

Endpoints
- POST `/api/chat`

Request
```json
{ "message": "Tôi cần đặt lịch tư vấn luật sư về đất đai", "userId": "<optional>" }
```

Response
```json
{ "answer": "...", "sources": ["/api/lawyers", "/api/appointments"] }
```

Configuration
- Environment variables (preferred):
  - `OpenAI__ApiKey` = your OpenAI key
  - `OpenAI__Model` = gpt-4o-mini (default)
  - `ServiceUrls__UsersAPI` = http://users-service
  - `ServiceUrls__LawyersAPI` = http://lawyers-service

Local run
```bash
dotnet run --project Chat.Services.API
```

Docker Compose
- Adds `chat-service` on port 7120
- Gateway routes `/api/chat` → chat-service


