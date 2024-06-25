Known missing features
======================

- `rules.json` reload on change
- automatic retry for failed forwards after some (progressive) timeout (and then… possibly… something like… automatic switch of the forwarding rule to a reject rule? Dunno)
- batch SMTP sending (do not forward mail-by-mail, with a separate full SMTP connection for each one; batch mails to a single SMTP connection)
- better crash tolerance (but mostly can/should be left to another layer, monitoring, etc.)
- obviously, UI, etc.
- reply handling instead of no-reply address

Upstream
--------

- `SmtpServer`’s `IMailbox` should have its own `ToString()` override, see `MailHeap.Service.Helpers.MailboxExtensions` and https://github.com/cosullivan/SmtpServer/pull/160
- `SmtpServer` should support&serve enhanced mail system status codes (RFC 3463)
