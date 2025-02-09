var Host = "http://192.168.1.13:8080";
var amountPerPage = 10;

var page = 0;
var NewsData = {};
// page number: key | list of objects: value

function onClickOptions() {
    console.log(window.location.href)
    if (window.location.href.endsWith('/') || window.location.href == Host) {
        window.location.href = "/options"

    } else {
        history.back()
    }
}

function handlePage() {
    // ◀ Page {page}
    //  Page {page} 
    // Page {page + 1} ▶ 

    let prevPage = document.getElementById("prev-page")
    let curPage = document.getElementById("cur-page")
    let nextPage = document.getElementById("next-page")

    if (page > 1) {
        prevPage.innerHTML = " ◀ Page "+(page - 1)+" "

    } else if (page == 1) {
        curPage.innerHTML = " Page "+ page + " "
        nextPage.innerHTML = " Page " + (page + 1)+" ▶ "
    }
}

function showNews(data) {
    let newsDiv = document.getElementById("news")

    let newNewsDiv = document.createElement("div")
    newNewsDiv.className = "news-view"

    let thumbnail = document.createElement("img")
    thumbnail.className = "news-thumbnail"
    thumbnail.src = Host + data.thumbnail_path

    newNewsDiv.appendChild(thumbnail)

    let title = document.createElement("p")
    title.className = "news-text"
    title.innerHTML = data.title
    newNewsDiv.appendChild(title)

    if (data.tags === null) {
        data.tags = ""
    }

    var splited = data.tags.split(';');

    var tagsDiv = document.createElement("div")
    tagsDiv.className = "news-tag-view"
    var count = 0;
    for (let i in splited) {
        let tagValue = splited[i]
        if (tagValue.length === 0) {
            continue
        }
        let tags = document.createElement("p")
        tags.className = "news-tags"
        tags.innerHTML = splited[i]
        tagsDiv.appendChild(tags)
        count++;
        if (count % 5 === 0) {
            newNewsDiv.appendChild(tagsDiv)
            tagsDiv = document.createElement("div")
        }
    }

    // posted_by_Admin_username
    let author = document.createElement("p")
    author.className = "news-author"
    author.innerHTML = "Posted By: " + data.posted_by_Admin_username
    newNewsDiv.appendChild(author)

    let posted_on = document.createElement("p")
    posted_on.className = "news-author"
    posted_on.innerHTML = "Posted on: " + new Date(data.posted_on).toLocaleString()
    newNewsDiv.appendChild(posted_on)

    let isFav = document.createElement("p")
    isFav.className = "news-fav"
    isFav.innerHTML = data.isFav ? "⭐" : ""
    newNewsDiv.appendChild(isFav)


    newsDiv.appendChild(newNewsDiv)
}

async function submitSearch(search, tags, authors) {
    let formData = new FormData()
    formData.append("tags", tags)
    formData.append("search", search)
    formData.append("post_authors", authors)
    formData.append("page", 1)
    formData.append("amount", amountPerPage)

    const res = await fetch(Host + "/news/search", {
        method: "POST",
        body: formData,
        redirect: "follow",
    })

    if (res.status !== 200) {
        alert("No success")
        return;
    }

    const data = await res.json();
    const News = data.News;
    if (News === null || News.length === 0) {
        alert("No news")
        return;
    }

    page = 1
    NewsData[page] = News
    handlePage()

    for (let x in News) {
        console.log(News[x])
        showNews(News[x])
    }
}

const searchParams = new URLSearchParams(window.location.search)

var searchV = searchParams.get('search')
var tags = searchParams.get('tags')
var authors = searchParams.get('authors')

if ((searchV !== null || tags !== null || authors !== null) &&
    (searchV.length > 0 || tags.length > 0 || authors.length > 0)) {
    submitSearch(searchV, tags, authors)
}

function goToSearchPage() {
    window.location.href = "/search"
}

// let searchV = document.getElementById("search_title").value
async function submitHomeSearch() {
    await submitSearch(document.getElementById("search_title").value,
        "", "")
}

