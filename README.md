# AppointmentSchedulingApp
Appointment scheduling application developed for my Advanced C# course.

# Application Description
* Validates user login before proceeding to the user dashboard.
* Determines users location and translates login page to Japanese if the user is located in Japan. Otherwise the page and corresponding error messages will display in English.
* Users can add, update, and delete customers records and appointments.
* After a successful login, an alert displays any upcoming appointments the user may have within 15 minutes of the login time.
* Exception controls implemented to prevent scheduling out-of-hours appointments and overlapping appointments.
* Appointments can be filtered by day, week, month, or all.
* Appointment times show in the users current timezone and account for daylight savings time.
* Stores information from the database in ObservableCollections to use as datasources for relevant datagrids.
* Records user timestamps in a .txt file to track user activity.
* Allows the user to generate reports to see important information. Such as, the schedule for each user, the number of appointment types by month, and the number of appointments per user.

# Technologies Used
* MySQL
* Windows Presentation Foundation (WPF)
* .NET Framework
