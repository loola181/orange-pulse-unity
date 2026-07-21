# Orange Football

Самостоятельное мобильное приложение на Unity 6 для спортивного расписания и новостей.
Проект создан с нуля и не использует исходники, пространства имён или архитектуру FutFest.

## Возможности

- ближайшие события из JSON API TheSportsDB;
- новости футбола, Формулы-1 и тенниса из официальных RSS-лент BBC Sport;
- серверный hero-баннер с изображением, текстом и CTA-кнопкой;
- выбор аватара через системную Android-галерею;
- локальный профиль, счётчики активности и дисковый кэш лент;
- офлайн-показ последних успешно загруженных данных;
- проверка всех внешних ссылок: приложение открывает только HTTPS.

## Серверный баннер

Приложение получает настройки баннера из Firebase Remote Config. Контент и CTA управляются
через общую панель Promo Hub без выпуска нового APK.

## Источники данных

- TheSportsDB v1: бесплатный ключ `123`, до 30 запросов в минуту;
- BBC Sport RSS: заголовки открываются на исходной странице BBC;
- удалённый баннер: Firebase Remote Config и Firebase Storage.

## Сборка Android

Требуется Unity `6000.3.10f1` с Android Build Support.

```bash
/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath "$PWD" \
  -executeMethod OrangePulse.Editor.OrangePulseBuild.BuildDevelopment \
  -logFile Build/unity-build.log
```

APK создаётся в `Build/OrangeFootball-dev.apk`. Идентификатор Android-приложения:
`com.loola181.orangefootball`.

## Структура

```text
Assets/OrangePulse/
├── Runtime/Core          адреса API и результат операций
├── Runtime/Data          HTTP, кэш, парсеры, профиль и изображения
├── Runtime/Native        мост Unity → Android gallery picker
├── Runtime/Presentation  UI-композиция, навигация и страницы
├── Plugins/Android       собственный Java picker без внешнего SDK
├── Editor                воспроизводимая подготовка сцены и сборка
└── Tests/EditMode        проверки RSS, remote config и профиля
```

## Приватность

Выбранное фото копируется только в `Application.persistentDataPath` на устройстве.
Проект не загружает аватар на сервер и не запрашивает прямой доступ ко всей медиатеке.
