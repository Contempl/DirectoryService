# DirectoryService

Локальный стенд монорепозитория: три .NET-сервиса, Next.js frontend и необходимая
инфраструктура запускаются одной командой через Docker Compose. Внешней точкой
входа для frontend и API служит nginx.

## Быстрый запуск

Для запуска достаточно Docker с поддержкой Compose. Локально устанавливать
.NET SDK, Node.js и PostgreSQL не требуется.

1. Создайте локальный файл конфигурации:

   ```powershell
   Copy-Item .env.example .env
   ```

   В bash аналогичная команда выглядит так:

   ```bash
   cp .env.example .env
   ```

2. Заполните в `.env` как минимум `NUGET_USERNAME` и `NUGET_PASSWORD` данными,
   позволяющими читать внутренние NuGet-пакеты проекта. При необходимости
   измените локальные пароли и порты. Пароль начального администратора должен
   удовлетворять требованиям ASP.NET Core Identity: содержать строчные и
   заглавные буквы, цифру и специальный символ.

3. Соберите и запустите стенд:

   ```bash
   docker compose up --build
   ```

   Для запуска в фоне добавьте `--detach`.

При старте сервисы автоматически применяют EF Core migrations к своим базам.
Готовность контейнеров можно проверить командой:

```bash
docker compose ps
```

## Внешние адреса

При значениях из `.env.example` доступны:

| Адрес | Назначение |
|---|---|
| `http://localhost/` | frontend через nginx |
| `http://localhost:9000` | S3 API через nginx; используется в presigned URL |
| `http://localhost:9001` | MinIO Console |
| `http://localhost:15672` | RabbitMQ Management |
| `http://localhost:8081` | Seq |

Порты задаются переменными `NGINX_PORT`, `MINIO_API_PORT`,
`MINIO_CONSOLE_PORT`, `RABBITMQ_MANAGEMENT_PORT` и `SEQ_UI_PORT`.
Если меняется `MINIO_API_PORT`, тот же порт нужно указать в
`MINIO_PUBLIC_ENDPOINT`, иначе S3-подпись и адрес для браузера не совпадут.

Backend-сервисы, PostgreSQL, Redis и RabbitMQ protocol port наружу не
публикуются. Контейнеры обращаются друг к другу по именам сервисов внутри сети
Compose.

Seq в этом локальном стенде запускается без аутентификации. Не публикуйте его
порт во внешнюю сеть.

## Маршруты nginx

| Внешний маршрут | Получатель |
|---|---|
| `/api/directory/*` | DirectoryService; префикс преобразуется в `/api/*` |
| `/api/auth/*` | AuthService |
| `/api/users*` | AuthService |
| `/api/files/*` | FileService |
| `/*` | Next.js frontend |

FileService обращается к MinIO по внутреннему адресу `http://minio:9000`, но
генерирует presigned URL с внешним `MINIO_PUBLIC_ENDPOINT`. Поэтому upload,
preview и download доступны браузеру и не содержат Docker hostname `minio`.

## Логи и управление стендом

Логи конкретного приложения:

```bash
docker compose logs directory-service
docker compose logs auth-service
docker compose logs file-service
```

Следить за логами в реальном времени можно с флагом `--follow`.

Остановить и удалить контейнеры, сохранив данные в volumes:

```bash
docker compose down
```

Полностью удалить стенд вместе с базами данных, файлами MinIO, очередями и
остальными persistent volumes:

```bash
docker compose down --volumes
```

Последняя команда безвозвратно удаляет локальные данные стенда. Обычный
`docker compose down` volumes не удаляет.

## Конфигурация и секреты

`.env.example` содержит только шаблон локальной конфигурации и коммитится в
репозиторий. Настоящий `.env` игнорируется Git. NuGet credentials передаются в
Docker build через BuildKit secrets и не объявляются аргументами Dockerfile.

Этот Compose предназначен для локальной разработки. Production TLS, внешний
secrets manager, Kubernetes и CI/CD находятся вне его scope.
