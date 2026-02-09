walk through a **tiny demo that prints thread IDs before and after `await`** to make it click instantly.

If you want, you can later use this as a checklist when deciding **Task vs ValueTask** for an API.


* Deep dive into `TaskScheduler`
* Compare `TaskCompletionSource` vs `async/await`
* Explain why `StartNew` breaks `await`



* Why `async void` is dangerous
* How exception handling differs in UI vs ASP.NET
* How `ConfigureAwait` affects exception flow

Blocking (`Wait`, `Result`, `GetAwaiter().GetResult()`) + async = potential deadlock
* Draw this deadlock with **actual thread IDs**
* Explain why ASP.NET Core “fixed” this
* Show how `ConfigureAwait` changes continuation flow
* WaitAsync
* Explain `async void` disasters