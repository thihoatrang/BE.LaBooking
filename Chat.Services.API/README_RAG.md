# RAG (Retrieval-Augmented Generation) Chat Service

## Tổng quan
Chat service đã được nâng cấp với RAG để AI có thể truy xuất thông tin từ dữ liệu thực của hệ thống (luật sư, khách hàng, lịch hẹn) thay vì chỉ dựa vào kiến thức có sẵn.

## Kiến trúc RAG
1. **Vector Store**: Redis lưu trữ embeddings và metadata
2. **Embedding Service**: Tạo vector từ text (dùng OpenAI embeddings hoặc hash fallback)
3. **Knowledge Service**: Index dữ liệu từ các microservices khác
4. **Retrieval Service**: Tìm kiếm thông tin liên quan dựa trên câu hỏi

## Endpoints mới

### Quản lý tri thức
- `POST /api/knowledge/rebuild` - Xây dựng lại toàn bộ knowledge base
- `POST /api/knowledge/index-lawyers` - Index dữ liệu luật sư
- `POST /api/knowledge/index-users` - Index dữ liệu khách hàng  
- `POST /api/knowledge/index-appointments` - Index dữ liệu lịch hẹn
- `POST /api/knowledge/index-custom` - Index dữ liệu tùy chỉnh
- `POST /api/knowledge/search` - Tìm kiếm trong knowledge base
- `DELETE /api/knowledge/clear` - Xóa toàn bộ knowledge base

### Chat (đã cập nhật)
- `POST /api/chat` - Chat với AI (giờ dùng RAG)

## Cách sử dụng

### 1. Khởi tạo Knowledge Base
```bash
# Xây dựng lại toàn bộ knowledge base
curl -X POST https://localhost:7120/api/knowledge/rebuild

# Hoặc index từng loại dữ liệu
curl -X POST https://localhost:7120/api/knowledge/index-lawyers
curl -X POST https://localhost:7120/api/knowledge/index-users
curl -X POST https://localhost:7120/api/knowledge/index-appointments
```

### 2. Thêm dữ liệu tùy chỉnh
```bash
curl -X POST https://localhost:7120/api/knowledge/index-custom \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Luật sư Nguyễn Văn A chuyên về dân sự, có 10 năm kinh nghiệm, giá 500k/giờ",
    "source": "manual",
    "metadata": {
      "category": "lawyer_profile",
      "specialization": "civil_law"
    }
  }'
```

### 3. Tìm kiếm thông tin
```bash
curl -X POST https://localhost:7120/api/knowledge/search \
  -H "Content-Type: application/json" \
  -d '{
    "query": "luật sư dân sự",
    "topK": 3
  }'
```

### 4. Chat với AI (dùng RAG)
```bash
curl -X POST https://localhost:7120/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Tôi cần tìm luật sư chuyên về dân sự",
    "userId": "user123"
  }'
```

## Cấu hình

### Redis
- Local: `localhost:6379`
- Docker: `redis:6379`
- Cấu hình trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### Embeddings
- Mặc định dùng OpenAI `text-embedding-3-small`
- Fallback: hash-based embedding nếu không có API key
- Cấu hình trong `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-key",
    "BaseUrl": "https://api.openai.com"
  }
}
```

## Docker Compose
Service đã được cập nhật để dùng Redis:
```yaml
chat-service:
  depends_on:
    - redis
  environment:
    - ConnectionStrings__Redis=redis:6379
```

## Lưu ý
- Knowledge base cần được khởi tạo trước khi chat
- Dữ liệu được index tự động từ các microservices khác
- Vector search dùng cosine similarity để tìm thông tin liên quan
- AI sẽ trả lời dựa trên thông tin tìm được thay vì kiến thức có sẵn
