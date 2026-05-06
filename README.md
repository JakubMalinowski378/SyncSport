# SyncSport

**SyncSport** jest systemem do zarządzania obiektami sportowymi zbudowany w architekturze **Modularnego Monolitu**. Dostarcza kompletne API backendowe do zarządzania obiektami sportowymi, kortami/boiskami, rezerwacjami, kontami użytkowników, cennikami, płatnościami oraz powiadomieniami.

System opiera się na zasadach **Domain-Driven Design (DDD)** oraz **Clean Architecture**, a jego logika biznesowa została zorganizowana w luźno powiązane, niezależne moduły.

## Technologie i Narzędzia

- **Platforma i Język:** .NET 10.0 (C#)
- **Baza danych:** PostgreSQL (Entity Framework Core + Npgsql)
- **Architektura API:** ASP.NET Core Minimal APIs z wykorzystaniem biblioteki **Carter**
- **Wzorce architektoniczne:** CQRS (z użyciem **MediatR**), DDD, Clean Architecture
- **Autentykacja i Autoryzacja:** JWT Bearer tokens
- **Walidacja:** FluentValidation
- **Magazyn plików:** Azure Blob Storage (lokalnie emulator Azurite)
- **Komunikacja Email:** SMTP (lokalnie smtp4dev oraz szablony Scriban)
- **Testy:** xUnit, NSubstitute, FluentAssertions
- **Konteneryzacja (środowisko deweloperskie):** Docker Compose (PostgreSQL, Azurite, smtp4dev)

## Architektura Systemu

### Modułowa Architektura (Modular Monolith)
Rozwiązanie zostało podzielone na **7 modułów**. Główne moduły realizują złożoną logikę biznesową zgodnie z Clean Architecture (podział na warstwy API, Application, Domain, Infrastructure), natomiast moduły wspierające posiadają bardziej zwięzłą architekturę ukierunkowaną na konkretne funkcje zadaniowe.

#### Główne Moduły:
1. **Users** - Uwierzytelnianie, autoryzacja oparta na rolach oraz zarządzanie kontami i prawami użytkowników.
2. **Facilities** - Moduł obiektów sportowych; obejmuje zarządzanie adresami, godzinami otwarcia oraz definiowanie poszczególnych kortów wewnątrz obiektu.
3. **Reservations** - Rezerwacje z wbudowanym systemem wykrywania konfliktów czasowych oraz zmianą statusów.
4. **Pricing** - Moduł cenników odpowiedzialny za wyliczanie opłat bazujących na dynamicznych regułach i taryfach (uwzględnianie godzin szczytu i weekendów).

#### Moduły Wspierające:
5. **Payments** - Moduł płatności (abstrakcja stanowiąca bramkę płatniczą).
6. **Notifications** - Moduł wysyłający komunikaty e-mail (m.in. resetowanie haseł) napędzany silnikiem szablonów.
7. **Storage** - Usługa do obsługi plików graficznych i zdjęć obiektów za pomocą Azure Blob.

### Shared Kernel (`src/Shared/`)
Elementy wspólne dla całego systemu ułatwiające spójność, w tym m.in:
- **Prymitywy domenowe**: Interfejsy i klasy bazowe takie jak `Entity<T>`, `AggregateRoot<T>`, `ValueObject`, `IDomainEvent`.
- **Infrastruktura i Trwałość danych**: Uogólnione repozytorium (Repository Pattern) pod EF Core, autowstrzykiwanie zdarzeń domenowych.
- **Zasady MediatR**: Pipeline Behaviors do automatycznej walidacji, wstrzykiwania aktywnego użytkownika, logowania.

## Uruchomienie projektu (Lokalnie)

1. Uruchom docker compose:
   ```bash
   docker-compose up -d
   ```
2. Uruchom aplikację:
   ```bash
   cd src/Bootstrapper
   dotnet run
   ```

## Frontend i Demo

- [Repozytorium frontendowe](https://github.com/JakubMalinowski378/SyncSport-frontend)
- [Demo](https://syncsport-czdxewevevbmbmb9.polandcentral-01.azurewebsites.net/)

## Konta Testowe

Hasło dla wszystkich kont to `Password123!`.

* **Użytkownik:** `user@syncsport.com` (Rola: User)
* **Manager:** `manager@syncsport.com` (Rola: Manager)
* **Administrator:** `admin@syncsport.com` (Rola: Admin)

## Płatności (Sandbox)

Moduł płatności podczas pracy w środowisku testowym wspiera fałszywą bramkę płatniczą. Do testowania skutecznych płatności użyj dowolnego kodu CVV, dowolnej przyszłej ważności karty oraz następującego numeru karty Sandbox:
* **Numer karty:** `4242 4242 4242 4242`
