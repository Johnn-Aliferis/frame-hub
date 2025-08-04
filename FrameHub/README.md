# FrameHub 📸

**FrameHub** is a backend service for a self-promoting social media platform designed to connect professionals and clients through curated visual portfolios. Users can sign up, subscribe, upload their work, and — based on an algorithm — be featured on the platform’s landing page, increasing their visibility to potential collaborators or customers.

---

## 🚀 Project Goals

FrameHub was built as a CV-grade project to demonstrate:

- Advanced backend architecture in **.NET Core**
- Clean, modular system design using **Domain-Driven Design**
- Integration with **Stripe**, **AWS S3**, and **RabbitMQ**
- Secure, scalable, real-world-ready infrastructure

While feature-complete for demonstration, certain edge-case implementations were left out intentionally to encourage deeper technical discussions around tradeoffs and priorities.

---

## 🧱 Architecture Overview

- ✅ Modular, clean architecture using `Domain`, `Application`, and `Infrastructure` layers
- ✅ Database-first approach using **Entity Framework Core (Fluent API)**
- ✅ Token-based authentication with **JWT**
- ✅ Support for **SSO via Google OAuth**
- ✅ External integrations: **Stripe**, **AWS S3**, **RabbitMQ**
- ✅ Asynchronous event processing via **background services**

---

## 📦 Modules

### 1. Authentication & Authorization

- Dual auth system:
    - Custom login/registration using **ASP.NET Core Identity**
    - **Google SSO** support (extensible for other OAuth providers)

- JWT bearer authentication with custom `[Claims]` attribute to extract `userId`, `email`, etc.
- Public vs protected route handling with `[AllowAnonymous]`
- Forward-thinking extensibility (e.g., Redis-based rate limiting planned)

---

### 2. Media Module

- Core business logic for uploading and confirming user media
- Upload access gated by **user subscription tier**
- Designed with provider-agnostic architecture:
    - Currently integrates with **Amazon S3**
    - Easily swappable via interface-based abstractions

- Scalable upload flow:
    - **Presigned URL** generation handled by backend
    - Uploads performed directly from frontend to S3
    - `/confirm` endpoint used to finalize and persist uploaded metadata

---

### 3. Subscriptions Module

- Payment flow powered by **Stripe** and the official **Stripe.NET SDK**
- Full lifecycle support:
    - Create, update, upgrade, downgrade, delete subscription

- **Stripe webhook** integration with:
    - Signature validation
    - Idempotency tracking via event persistence
    - Routing based on event type (`subscription.updated`, `invoice.payment_failed`, etc.)

- **RabbitMQ** used as an intermediary between Stripe webhook and internal logic:
    - Prevents long request chains
    - Ensures reliability and fault tolerance
    - Enables async, scalable processing
    - **This approach keeps the project clean while showcasing message-driven design where it provides value.**

- Alternative architecture considerations (e.g., using scheduled billing with one-time payments) acknowledged for real-world tradeoff discussions

---

### 4. Shared Module

- Contains cross-cutting utilities, configs, and abstractions:
    - Database migration SQL scripts (for DB-first setup)
    - Global custom exception types
    - Shared interfaces and DTOs
- Supports clean modular boundaries and reusability

---

## 🧪 Testing

### Unit Testing
- Focus on method-level and service-level logic
- Architecture designed for testability using **dependency injection** and **abstractions**
- Partial test coverage aimed at demonstrating testing capability and clean isolation

### Manual & Integration Testing
- **Stripe CLI** used to simulate webhook payloads
- **Test clock** advanced to simulate subscription life cycle
- **Stripe test cards** used to verify success/failure paths

---

## 🔧 Technologies Used

- **.NET Core 8**
- **Entity Framework Core (Fluent API)**
- **ASP.NET Core Identity**
- **Stripe.NET SDK**
- **Amazon S3 SDK**
- **RabbitMQ + Hosted Background Services**
- **Microsoft SQL**
- **Docker Compose**
- **xUnit & Moq**

---

## 💡 Future Enhancements (Left Out Intentionally)

This project focuses on foundational architecture. The following are known enhancements left out deliberately for discussion:

- API **rate limiting** via Redis
- **Caching** strategies to reduce DB load
- **Email/push notifications** for system events
- **CDN integration** for static asset delivery
- **Landing page media selection algorithm**

---

## 📫 Contact

This project was built by Ioannis - Panagiotis Aleiferis as part of a portfolio to showcase **senior-level backend skills**, **system design ability**, and **stack flexibility** (transition from Java/Spring to C#/.NET Core).
Feel free to reach out for collaboration or discussion.

---