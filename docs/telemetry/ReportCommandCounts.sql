SELECT eventname as Command, count(*) as Count
  FROM telemetryevents
 GROUP BY eventname
 ORDER BY Count DESC;