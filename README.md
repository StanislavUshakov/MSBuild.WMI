# MSBuild.WMI
Custom MSBuild Tasks for managing IIS

Now 2 tasks are available:
1 - AppPool: CheckExists/Create/Start/Stop Application Pool
2 - WebSite: CheckExists/Create/Start/Stop Web Site

Example form deploy.proj:
```xml
<MSBuild.WMI.AppPool TaskAction="CheckExists" AppPoolName="$(AppPoolName)" Machine="$(Machine)" UserName="$(User)" Password="$(Password)">
  <Output TaskParameter="Exists" PropertyName="AppPoolExists"/>
</MSBuild.WMI.AppPool>
```
