pipeline {
    agent any
    options {
        timeout(time: 1, unit: 'HOURS') 
        timestamps()
    }
    stages {
        stage('Build & Integration Test1') {
            steps {
                sh "docker-compose -f docker-compose-db.yml up"
                sh "docker-compose -f docker-compose.integration.yml up --abort-on-container-exit"
                sh "docker-compose -f docker-compose.integration.yml down -v"
            }
        }
        stage('Build & Unit Test') {
            steps {
                slackSend color: "good", message : "Build started - Job - ${env.JOB_NAME} Build Number - ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
                sh "docker build -t accountownerapp:B${BUILD_NUMBER} -f Dockerfile ."
                sh "docker build -t accountownerapp:test-B${BUILD_NUMBER} -f Dockerfile.Integration ."
            }
        }
        stage('Publish Unit Testing & Code Coverage Reports'){
            steps {
                script {
                    containerID = sh (
                    script: "docker run -d accountownerapp:B${BUILD_NUMBER}", 
                    returnStdout: true
                    ).trim()
                    echo "Container ID is ==> ${containerID}"
                    sh "docker cp ${containerID}:/TestResults/test_results.trx test_results.trx"
                    sh "docker cp ${containerID}:/TestResults/coverage.xml coverage.xml"
                    sh "docker stop ${containerID}"
                    sh "docker rm ${containerID}"
                    step([$class: 'MSTestPublisher', failOnError: true, testResultsFile: '**/test_results.trx'])
                    step([$class: 'CoberturaPublisher', autoUpdateHealth: false, autoUpdateStability: false, coberturaReportFile: '**/coverage.xml', failUnhealthy: false, failUnstable: false, maxNumberOfBuilds: 0, onlyStable: false, sourceEncoding: 'ASCII', zoomCoverageChart: false])
                }
            }
        }
        stage('Build & Integration Test') {
            steps {
                sh "docker-compose -f docker-compose.integration.yml up --force-recreate --abort-on-container-exit"
                sh "docker-compose -f docker-compose.integration.yml down -v"
            }
        }
        stage('Pushing Conatiners to Registry') {
            environment {
                ACR_CREDS = credentials('acr-credential')
            }
            steps {
                script {
                    WEB_IMAGE_NAME="${env.ACR_LOGINSERVER}/accountownerapp:B${BUILD_NUMBER}"
                    sh "docker tag accountownerapp:B${BUILD_NUMBER} $WEB_IMAGE_NAME"
                    sh "docker login ${env.ACR_LOGINSERVER} -u ${ACR_CREDS_USR} -p ${ACR_CREDS_PSW}"
                    sh "docker push $WEB_IMAGE_NAME"
                }
            }
        }
        stage('Deploying to Kubernetes cluster') {
            steps {
                script {
                    WEB_IMAGE_NAME="${env.ACR_LOGINSERVER}/accountownerapp:B${BUILD_NUMBER}"
                    DEPLOYMENT_NAME="deployment/accountownerapp-api"
                    sh "kubectl rollout pause $DEPLOYMENT_NAME --kubeconfig /var/lib/jenkins/config || true"
                    sh "kubectl set image $DEPLOYMENT_NAME accountownerapp=$WEB_IMAGE_NAME --kubeconfig /var/lib/jenkins/config"
                    sh "kubectl rollout resume $DEPLOYMENT_NAME --kubeconfig /var/lib/jenkins/config"
                }
            }
        }
        // stage('Publish Integration Testing Report'){
        //     steps {
        //         script {
        //             containerID = sh (
        //             script: "docker run -d accountownerapp:B${BUILD_NUMBER}", 
        //             returnStdout: true
        //             ).trim()
        //             echo "Container ID is ==> ${containerID}"
        //             sh "docker cp ${containerID}:/TestResults/coverage.trx coverage.trx"
        //             sh "docker stop ${containerID}"
        //             sh "docker rm ${containerID}"
        //             step([$class: 'MSTestPublisher', failOnError: true, testResultsFile: '**/*.xml'])
        //         }
        //     }
        // }
    }
    post {
        success {
            slackSend color: "good", message : "Build finished sucessfully - Job - ${env.JOB_NAME} Build Number - ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
        }
        failure {
            slackSend color: "#439FE0", message : "Build failed - Job - ${env.JOB_NAME} Build Number - ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
        }
    }
}