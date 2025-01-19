Known missing features
======================

- automatic retry for failed forwards after some (progressive) timeout (and then… possibly… something like… automatic switch of the forwarding rule to a reject rule? Dunno)
- batch SMTP sending (do not forward mail-by-mail, with a separate full SMTP connection for each one; batch mails to a single SMTP connection)
- better crash tolerance (but mostly can/should be left to another layer, monitoring, etc.)
- obviously, UI, etc.
- reply handling instead of no-reply address
- instead of a single static certificate, support certificate renewals:
  - implement AutoReloadingCertificate : ICertificateFactory, which (similarly to AutoReloadingRuleCollection) watches for changes in the certificate file(s), and if change is detected, reloads the certificate, while serving the last successfully loaded X509Certificate for new sessions

Upstream
--------

- `SmtpServer`’s `IMailbox` should have its own `ToString()` override, see `MailHeap.Service.Helpers.MailboxExtensions` and https://github.com/cosullivan/SmtpServer/pull/160
- `SmtpServer` should support&serve enhanced mail system status codes (RFC 3463)
