package we.newsapp.newsappserver;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;
import we.newsapp.newsappserver.database.DatabaseManager;
import we.newsapp.newsappserver.models.News;

import java.util.Collections;

@SpringBootApplication(exclude = {DataSourceAutoConfiguration.class })
public class NewsappserverApplication {
	public static DatabaseManager databaseManager;

	public static void main(String[] args) {
		// Set password: 1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3
		// Command: INSERT INTO Admins VALUES ('1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3');
		//




		databaseManager = new DatabaseManager();
		try {
			databaseManager.setup();
		} catch (Exception e) {
			System.out.println(e.getMessage());
			return;
		}


		SpringApplication app = new SpringApplication(NewsappserverApplication.class);
		app.setDefaultProperties(Collections.singletonMap("server.port", 3));
		app.run(args);
	}

}


