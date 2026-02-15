# DevBoard

project management for devs who actually code. built this cause jira is too slow and generic.

## prerequisite

you need visual studio 2019 or newer with the asp.net web development workload installed. make sure .net framework 4.8.1 is on your machine.

## installation

1. clone the repo
2. open `DevBoard.sln` in visual studio
3. rebuild solution (ctrl+shift+b) to restore nuget packages
4. hit f5 to run. the app will auto-create the db and seed data on first run.

## features

we got differnt roles cause access control matters:
*   **Admin**: manages users and general config
*   **Dev**: full access to projects, kanban boards, code integration
*   **QA**: focused on bug tracking, test coverage, and dashboards
*   **Stakeholder**: read only access to see progress reports

core functionality:
*   **GitHub Integration**: sync modules and repos directly from github
*   **Kanban Board**: drag and drop tickets across status columns
*   **QA Dashboard**: metrics on test coverage and flaky tests
*   **Reporting**: visualize project health and debt

## integrations

*   **GitHub**: seamless sync for project structure and modules

## disclaimer

this is currently a college project so do not trip if it breaks or looks a bit rough around the edges. the tech stack is kinda legacy (webforms lol) but it works.

planning to rewrite everything to .net 8 and react in the next iteration to make it performant and scale properly. until then, enjoy the retro vibes
