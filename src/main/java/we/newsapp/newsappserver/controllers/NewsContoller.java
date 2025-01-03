package we.newsapp.newsappserver.controllers;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import we.newsapp.newsappserver.models.SaneBB;
import we.newsapp.newsappserver.models.News;

@RestController
public class NewsContoller {

    @GetMapping("/news")
    public News getAllNews(@RequestParam(value = "limit", defaultValue = "0") int limit) {
        return new News();
    }

    @PostMapping("/news")
    public void s(String a) {
        SaneBB.Result b = SaneBB.parse(a);
        System.out.println(b.html);
    }
}
