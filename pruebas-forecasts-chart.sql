SELECT forecast_id,
       user_id,
       match_id,
       predicted_home_score,
       predicted_away_score,
       submitted_at
FROM user_25.forecasts
where match_id = 10
LIMIT 1000;

select * from user_25.forecasts
where match_id = 10;

delete from user_25.forecasts
where forecast_id = 30;

insert into user_25.forecasts (user_id, match_id, predicted_home_score, predicted_away_score, submitted_at)
values (3, 10, 0, 0, now());
commit;