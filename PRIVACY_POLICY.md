# Политика конфиденциальности Orange Football

**Дата вступления в силу:** 21 июля 2026 года<br>
**Последнее обновление:** 21 июля 2026 года<br>
**Версия:** 1.0

Настоящая Политика конфиденциальности описывает обработку данных в мобильном приложении **Orange Football** для Android (`app.orangefootball.fans`). Оператор приложения — **Orange Football Studio**. Контакт по вопросам конфиденциальности: [psycheftw@gmail.com](mailto:psycheftw@gmail.com).

## 1. Какие данные обрабатывает приложение

### Данные, которые остаются на устройстве

Приложение может сохранять в закрытом хранилище приложения:

- указанный пользователем никнейм;
- выбранную любимую лигу;
- счётчики активности внутри приложения;
- фотографию профиля, выбранную пользователем через системный интерфейс выбора документов.

Эти данные не загружаются на серверы Orange Football Studio. Выбранное изображение декодируется, уменьшается и сохраняется как локальная копия в хранилище приложения. Приложение не запрашивает постоянный доступ ко всей галерее.

### Технические и сетевые данные

Для загрузки спортивных данных, новостей, эмблем клубов, параметров интерфейса и промоматериалов приложение обращается к интернет-сервисам. При обычном сетевом соединении поставщики могут получать IP-адрес, User-Agent, время запроса, версию приложения и иные стандартные технические сведения.

Приложение использует Firebase Remote Config и Google Analytics for Firebase. В зависимости от настроек устройства и сервисов Google могут обрабатываться:

- Firebase Installation ID и идентификатор экземпляра приложения;
- рекламный идентификатор Android, если он доступен;
- сведения об устройстве, ОС, языке, стране или примерном регионе, версии и package name приложения;
- данные о запуске, сеансах и взаимодействиях с приложением;
- сведения об установке и источнике установки;
- технические и диагностические данные.

Приложение не использует эти сведения для самостоятельного определения личности пользователя и не продаёт персональные данные.

## 2. Для чего используются данные

Данные обрабатываются, чтобы:

- показывать расписание, результаты, таблицы, бомбардиров и новости;
- загружать эмблемы, изображения и данные матч-центра;
- получать через Firebase актуальные настройки баннеров и промоматериалов;
- поддерживать локальный профиль пользователя;
- оценивать стабильность, использование и качество приложения;
- предотвращать злоупотребления и обеспечивать безопасность сервисов.

## 3. Поставщики и получатели данных

Для работы приложения используются:

- **Google Firebase / Google Analytics** — конфигурация, идентификатор установки и аналитика: [Privacy and Security in Firebase](https://firebase.google.com/support/privacy), [Google Privacy Policy](https://policies.google.com/privacy);
- **Google Cloud Functions** — проксирование части спортивных запросов;
- **TheSportsDB** — спортивные данные и изображения: [Privacy Policy](https://www.thesportsdb.com/docs_privacy_policy.php);
- **BBC Sport RSS** — новостные материалы: [BBC Privacy Policy](https://www.bbc.co.uk/usingthebbc/privacy/);
- **GitHub Pages** — размещение публичной версии этой политики: [GitHub Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement).

Передача указанным поставщикам выполняется только в объёме, необходимом для соответствующей функции. Поставщики обрабатывают данные по собственным условиям и политикам. Обработка может выполняться в странах, отличных от страны пользователя.

## 4. Изображения и разрешения устройства

Для выбора аватара используется системный Android `ACTION_OPEN_DOCUMENT`. Пользователь самостоятельно выбирает конкретное изображение. Приложение не запрашивает доступ к контактам, микрофону, камере, точной геолокации или ко всем файлам устройства.

## 5. Промоматериалы и внешние ссылки

Приложение может показывать промобаннер, параметры которого загружаются из Firebase Remote Config. Нажатие на баннер или новость может открыть HTTPS-ссылку во внешнем браузере. После перехода действует политика конфиденциальности соответствующего сайта. В приложение не встроена сторонняя рекламная сеть.

## 6. Хранение и удаление

Локальный профиль, кеш и выбранное изображение хранятся до очистки данных приложения или его удаления. Orange Football не создаёт пользовательские аккаунты.

Удалить локальные данные можно через системные настройки Android, очистив хранилище Orange Football, либо удалив приложение. Для запросов, связанных с данными, обрабатываемыми поставщиками от имени Orange Football Studio, напишите на [psycheftw@gmail.com](mailto:psycheftw@gmail.com). В запросе не следует отправлять пароли или копии документов.

Сроки хранения технических данных определяются назначением обработки, настройками проекта и политиками соответствующих поставщиков. Firebase Installation ID хранится Google в соответствии с правилами Firebase Remote Config и может быть заменён после очистки данных или переустановки приложения.

## 7. Безопасность

Сетевые адреса приложения используют HTTPS. Локальные данные сохраняются в закрытом хранилище приложения Android. Мы принимаем разумные меры защиты, однако ни один способ передачи или хранения данных не гарантирует абсолютную безопасность.

## 8. Дети

Orange Football не предназначен специально для детей младше 13 лет и не предусматривает создание аккаунта. Мы сознательно не запрашиваем у детей контактные данные. Если вы считаете, что ребёнок передал персональные данные, свяжитесь с нами для рассмотрения запроса.

## 9. Права пользователя

В зависимости от применимого законодательства пользователь может запросить доступ, удаление, ограничение обработки или возразить против обработки данных. Для обращения используйте [psycheftw@gmail.com](mailto:psycheftw@gmail.com). Также пользователь может управлять рекламным идентификатором и настройками конфиденциальности в Android и аккаунте Google.

## 10. Изменения политики

Политика может обновляться при изменении функций приложения, поставщиков или требований законодательства. На этой странице всегда указывается дата последнего обновления. Существенные изменения могут дополнительно сообщаться в приложении или на странице магазина.

## 11. Контакты

**Orange Football Studio**<br>
Приложение: **Orange Football**<br>
Package name: `app.orangefootball.fans`<br>
Email: [psycheftw@gmail.com](mailto:psycheftw@gmail.com)

---

# Orange Football Privacy Policy

**Effective date:** July 21, 2026<br>
**Last updated:** July 21, 2026<br>
**Version:** 1.0

This Privacy Policy explains how the **Orange Football** Android application (`app.orangefootball.fans`) handles data. The application is operated by **Orange Football Studio**. Privacy contact: [psycheftw@gmail.com](mailto:psycheftw@gmail.com).

## Data handled locally

Your nickname, favorite league, in-app activity counters, and selected profile image are stored in the app's private local storage. The selected image is obtained through the Android system document picker, resized, and saved as a local copy. Orange Football Studio does not upload this profile information or image to its servers.

## Network and technical data

The app connects to sports, news, image, Firebase, and cloud services. Service providers may receive standard network information such as IP address, User-Agent, request time, app version, and device information.

Firebase Remote Config and Google Analytics for Firebase may process a Firebase Installation ID, app-instance identifiers, Android Advertising ID when available, device and operating-system information, language, approximate region, app version, installation source, sessions, app interactions, and technical diagnostics. We do not sell personal data or use this information to independently identify users.

## Purposes

Data is used to deliver sports schedules, results, standings, scorers, match-center information, news, badges and images; retrieve remote banner settings; maintain the local profile; understand app usage and stability; and protect services from abuse.

## Service providers

The app uses [Google Firebase](https://firebase.google.com/support/privacy), [Google services](https://policies.google.com/privacy), [TheSportsDB](https://www.thesportsdb.com/docs_privacy_policy.php), [BBC Sport](https://www.bbc.co.uk/usingthebbc/privacy/), and Google Cloud Functions. This policy is hosted with [GitHub Pages](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement). Providers handle data under their own terms and may process it internationally.

## Device access and external links

The app accesses only the image explicitly selected through the Android system picker. It does not request access to contacts, microphone, camera, precise location, or all device files. News and promotional links may open in an external browser, where the destination site's policy applies. No third-party advertising network is embedded in the app.

## Retention, deletion, and rights

Orange Football does not create user accounts. Local data remains until app storage is cleared or the app is uninstalled. To request assistance concerning data processed on behalf of Orange Football Studio, email [psycheftw@gmail.com](mailto:psycheftw@gmail.com). Depending on applicable law, users may request access, deletion, restriction, or objection to processing and may manage Advertising ID and Google privacy settings through Android and their Google account.

## Security and children

The app uses HTTPS endpoints and Android private app storage. No transmission or storage system is completely secure. Orange Football is not specifically directed to children under 13, and we do not knowingly request children's contact information.

## Changes and contact

We may update this policy when the app, providers, or legal requirements change. The current revision date will appear on this page.

**Orange Football Studio**<br>
Application: **Orange Football**<br>
Package name: `app.orangefootball.fans`<br>
Email: [psycheftw@gmail.com](mailto:psycheftw@gmail.com)
