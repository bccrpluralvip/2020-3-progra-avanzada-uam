-- show team osasuna data
select * from teams where name = 'Osasuna';
-- show real madrid data
select * from teams where name = 'Real Madrid';
-- show osasuna versus real madrid match data
select * from matches where (home_team_id = '3' and away_team_id = '1');
-- show forecast for osasuna versus real madrid match
select * from forecasts where match_id = 11;
-- delete forecast for osasuna versus real madrid match
delete from forecasts where match_id = 11;
commit;
begin transaction;
-- insert forecast for osasuna versus real madrid match
insert into forecasts (match_id, user_id, predicted_home_score, predicted_away_score) values (11, 1, 3, 1);
update forecasts set predicted_home_score = 2, predicted_away_score = 1 where match_id = 11 and user_id = 1;
commit;

/*


*/